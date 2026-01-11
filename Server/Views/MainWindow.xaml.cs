namespace Server.Views;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using Server.Services;
using Shared.Protocol;
using Shared.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class MainWindow : Window
{
    private TcpServerService _tcpServer;
    private BillingEngine _billingEngine;
    private DatabaseManager _db;
    private ObservableCollection<ClientViewModel> _clients;
    private System.Timers.Timer _uiUpdateTimer;
    private ServerConfig _config;
    
    public MainWindow()
    {
        InitializeComponent();
        LoadConfig();
        InitializeServices();
        StartServer();
        
        _clients = new ObservableCollection<ClientViewModel>();
        ClientGrid.ItemsSource = _clients;
        
        // UI update timer
        _uiUpdateTimer = new System.Timers.Timer(1000);
        _uiUpdateTimer.Elapsed += UpdateUI;
        _uiUpdateTimer.Start();
    }
    
    private void LoadConfig()
    {
        try
        {
            string json = System.IO.File.ReadAllText("config.json");
            _config = JsonConvert.DeserializeObject<ServerConfig>(json) ?? new ServerConfig();
        }
        catch
        {
            _config = new ServerConfig();
            Logger.Warning("Using default configuration");
        }
    }
    
    private void InitializeServices()
    {
        // Database
        _db = new DatabaseManager(_config.Database.Path);
        
        // TCP Server
        _tcpServer = new TcpServerService();
        _tcpServer.OnClientConnected += OnClientConnected;
        _tcpServer.OnClientDisconnected += OnClientDisconnected;
        _tcpServer.OnMessageReceived += OnMessageReceived;
        
        // Billing Engine
        _billingEngine = new BillingEngine(_tcpServer, _db);
        _billingEngine.OnSessionExpired += OnSessionExpired;
        _billingEngine.OnSessionWarning += OnSessionWarning;
    }
    
    private void StartServer()
    {
        try
        {
            _tcpServer.Start(_config.Server.Port);
            _billingEngine.Start();
            
            Logger.Info($"Server started on port {_config.Server.Port}");
            ServerStatusText.Text = "🟢 Server: Running";
            ServerStatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to start server: {ex.Message}");
            MessageBox.Show($"Gagal start server: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void OnClientConnected(ConnectedClient client)
    {
        Dispatcher.Invoke(() =>
        {
            Logger.Info($"Client connected: {client.Id} ({client.IpAddress})");
            
            var viewModel = new ClientViewModel
            {
                ClientId = client.Id,
                Name = client.Name ?? $"PC-{client.Id}",
                IpAddress = client.IpAddress,
                Status = "Idle",
                StatusColor = "#95a5a6"
            };
            
            _clients.Add(viewModel);
            
            // Save to database
            _db.UpsertClient(new Shared.Models.Client
            {
                Id = client.Id,
                Name = viewModel.Name,
                IpAddress = client.IpAddress,
                IsOnline = true,
                LastSeen = DateTime.Now
            });
        });
    }
    
    private void OnClientDisconnected(ConnectedClient client)
    {
        Dispatcher.Invoke(() =>
        {
            var viewModel = _clients.FirstOrDefault(c => c.ClientId == client.Id);
            if (viewModel != null)
            {
                viewModel.Status = "Offline";
                viewModel.StatusColor = "#7f8c8d";
                
                // Update database
                _db.UpsertClient(new Shared.Models.Client
                {
                    Id = client.Id,
                    Name = viewModel.Name,
                    IpAddress = client.IpAddress,
                    IsOnline = false,
                    LastSeen = DateTime.Now
                });
            }
        });
    }
    
    private void OnMessageReceived(ConnectedClient client, Message message)
    {
        Dispatcher.Invoke(() =>
        {
            switch (message.Type)
            {
                case "CLIENT_LOGIN":
                    HandleClientLogin(client, message);
                    break;
                    
                case MessageType.HEARTBEAT:
                    // Update last seen
                    var vm = _clients.FirstOrDefault(c => c.ClientId == client.Id);
                    if (vm != null) vm.LastSeen = DateTime.Now;
                    break;
                    
                case MessageType.IDLE_DETECTED:
                    HandleIdleDetection(client, message);
                    break;
            }
        });
    }
    
    private async void HandleClientLogin(ConnectedClient client, Message message)
    {
        var data = JObject.FromObject(message.Data ?? new { });
        string username = data["username"]?.Value<string>() ?? "GUEST";
        bool isGuest = data["is_guest"]?.Value<bool>() ?? false;
        
        client.Name = data["client_id"]?.Value<string>() ?? $"PC-{client.Id}";
        
        var viewModel = _clients.FirstOrDefault(c => c.ClientId == client.Id);
        if (viewModel != null)
        {
            viewModel.Name = client.Name;
            viewModel.Username = username;
        }
        
        await client.SendAsync(new Message
        {
            Type = "LOGIN_SUCCESS",
            Data = new
            {
                message = isGuest 
                    ? "Login sebagai guest berhasil!" 
                    : $"Selamat datang, {username}!"
            }
        });
        
        Logger.Info($"Client {client.Name} logged in as {username}");
    }
    
    private void HandleIdleDetection(ConnectedClient client, Message message)
    {
        Logger.Warning($"Client {client.Id} is idle");
        
        // Optional: Auto-pause session
        if (_billingEngine.ActiveSessions.ContainsKey(client.Id))
        {
            var result = MessageBox.Show(
                $"Client {client.Name} terdeteksi idle. Pause sesi?",
                "Idle Detected",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _billingEngine.PauseSession(client.Id);
            }
        }
    }
    
    private void OnSessionExpired(ActiveSession session)
    {
        Dispatcher.Invoke(() =>
        {
            Logger.Info($"Session expired for {session.ClientName}");
            
            MessageBox.Show(
                $"Waktu habis untuk {session.ClientName}\nTotal: Rp {session.TotalCost:N0}",
                "Session Expired",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            RefreshClientList();
        });
    }
    
    private void OnSessionWarning(ActiveSession session)
    {
        Dispatcher.Invoke(() =>
        {
            Logger.Warning($"5 minutes warning for {session.ClientName}");
            // Could play sound or show notification
        });
    }
    
    private void UpdateUI(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Update revenue
            RevenueText.Text = $"Rp {_db.GetDailyRevenue():N0}";
            MonthlyRevenueText.Text = $"Rp {_db.GetMonthlyRevenue():N0}";
            
            // Update active sessions
            foreach (var client in _clients)
            {
                if (_billingEngine.ActiveSessions.TryGetValue(client.ClientId, out var session))
                {
                    client.Status = session.IsPaused ? "Paused" : "Active";
                    client.StatusColor = session.IsPaused ? "#f39c12" : "#2ecc71";
                    client.RemainingTime = session.RemainingTimeFormatted;
                    client.CurrentCost = $"Rp {session.TotalCost:N0}";
                }
            }
        });
    }
    
    private void OnStartClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        string clientId = button?.Tag?.ToString() ?? "";
        
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);
        if (client == null) return;
        
        // Show start session dialog
        var dialog = new StartSessionDialog(_db, clientId, client.Name);
        if (dialog.ShowDialog() == true)
        {
            _billingEngine.StartSession(
                clientId, 
                client.Name, 
                dialog.Minutes, 
                dialog.RatePerHour);
            
            Logger.Info($"Started session: {client.Name} - {dialog.Minutes}min @ Rp{dialog.RatePerHour}/hr");
        }
    }
    
    private void OnPauseClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        string clientId = button?.Tag?.ToString() ?? "";
        
        _billingEngine.PauseSession(clientId);
    }
    
    private void OnResumeClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        string clientId = button?.Tag?.ToString() ?? "";
        
        _billingEngine.ResumeSession(clientId);
    }
    
    private void OnExtendClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        string clientId = button?.Tag?.ToString() ?? "";
        
        _billingEngine.ExtendTime(clientId, 30);
        
        MessageBox.Show("Waktu ditambah 30 menit", "Success", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private void OnStopClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        string clientId = button?.Tag?.ToString() ?? "";
        
        var result = MessageBox.Show(
            "Yakin mau stop sesi ini?",
            "Confirm Stop",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            _billingEngine.EndSession(clientId);
        }
    }
    
    private void OnPaymentClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        string clientId = button?.Tag?.ToString() ?? "";
        
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);
        if (client == null) return;
        
        var paymentDialog = new PaymentDialog(clientId, client.Name);
        paymentDialog.ShowDialog();
    }
    
    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        RefreshClientList();
    }
    
    private void RefreshClientList()
    {
        // Reload from database
        var dbClients = _db.GetAllClients();
        
        // Update existing or add new
        foreach (var dbClient in dbClients)
        {
            var existing = _clients.FirstOrDefault(c => c.ClientId == dbClient.Id);
            if (existing == null)
            {
                _clients.Add(new ClientViewModel
                {
                    ClientId = dbClient.Id,
                    Name = dbClient.Name,
                    IpAddress = dbClient.IpAddress ?? "",
                    Status = dbClient.IsOnline ? "Idle" : "Offline",
                    StatusColor = dbClient.IsOnline ? "#95a5a6" : "#7f8c8d"
                });
            }
        }
    }
    
    private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        // Implement filtering logic
    }
    
    private void OnAddClientClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ClientManagement(_db);
        dialog.ShowDialog();
        RefreshClientList();
    }
    
    private void OnPricingClick(object sender, RoutedEventArgs e)
    {
        var pricingWindow = new PricingWindow(_db);
        pricingWindow.ShowDialog();
    }
    
    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }
    
    private void OnReportsClick(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new ReportsWindow(_db);
        reportsWindow.Show();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _uiUpdateTimer?.Stop();
        _billingEngine?.Stop();
        _tcpServer?.Stop();
        base.OnClosed(e);
    }
}

// ViewModel for DataGrid
public class ClientViewModel : System.ComponentModel.INotifyPropertyChanged
{
    private string _status = "";
    private string _remainingTime = "-";
    private string _currentCost = "-";
    
    public string ClientId { get; set; } = "";
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Username { get; set; } = "Guest";
    public DateTime LastSeen { get; set; }
    
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(nameof(Status)); UpdateButtonVisibility(); }
    }
    
    public string StatusColor { get; set; } = "#95a5a6";
    
    public string RemainingTime
    {
        get => _remainingTime;
        set { _remainingTime = value; OnPropertyChanged(nameof(RemainingTime)); }
    }
    
    public string CurrentCost
    {
        get => _currentCost;
        set { _currentCost = value; OnPropertyChanged(nameof(CurrentCost)); }
    }
    
    // Button visibility
    public Visibility ShowStartButton => Status == "Idle" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowPauseButton => Status == "Active" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowResumeButton => Status == "Paused" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowExtendButton => Status == "Active" || Status == "Paused" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowStopButton => Status == "Active" || Status == "Paused" ? Visibility.Visible : Visibility.Collapsed;
    
    private void UpdateButtonVisibility()
    {
        OnPropertyChanged(nameof(ShowStartButton));
        OnPropertyChanged(nameof(ShowPauseButton));
        OnPropertyChanged(nameof(ShowResumeButton));
        OnPropertyChanged(nameof(ShowExtendButton));
        OnPropertyChanged(nameof(ShowStopButton));
    }
    
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}

// Config class
public class ServerConfig
{
    public ServerSettings Server { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
}

public class ServerSettings
{
    public int Port { get; set; } = 9999;
}

public class DatabaseSettings
{
    public string Path { get; set; } = "Data/billing.db";
}