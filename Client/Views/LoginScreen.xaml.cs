namespace Client.Views;

using System;
using System.Windows;
using System.Windows.Input;
using Client.Services;
using Shared.Protocol;
using Shared.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class LoginScreen : Window
{
    private TcpClientService _tcpClient;
    private ClientConfig _config;
    private bool _isConnected = false;
    
    public LoginScreen()
    {
        InitializeComponent();
        LoadConfig();
        InitializeTcpClient();
        
        // Focus username input
        Loaded += (s, e) => UsernameInput.Focus();
    }
    
    private void LoadConfig()
    {
        try
        {
            string json = System.IO.File.ReadAllText("config.json");
            _config = JsonConvert.DeserializeObject<ClientConfig>(json) ?? new ClientConfig();
        }
        catch
        {
            _config = new ClientConfig
            {
                Server = new ServerSettings { Ip = "127.0.0.1", Port = 9999 },
                Client = new ClientSettings { Name = "PC-01" }
            };
        }
    }
    
    private async void InitializeTcpClient()
    {
        _tcpClient = new TcpClientService(_config.Server.Ip, _config.Server.Port);
        
        _tcpClient.OnConnected += () =>
        {
            Dispatcher.Invoke(() =>
            {
                _isConnected = true;
                ServerStatusText.Text = "🟢 Server: Connected";
                ServerStatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;
                LoginButton.IsEnabled = true;
            });
        };
        
        _tcpClient.OnDisconnected += () =>
        {
            Dispatcher.Invoke(() =>
            {
                _isConnected = false;
                ServerStatusText.Text = "🔴 Server: Disconnected";
                ServerStatusText.Foreground = System.Windows.Media.Brushes.Red;
                LoginButton.IsEnabled = false;
                ShowError("Koneksi ke server terputus. Mencoba reconnect...");
            });
        };
        
        _tcpClient.OnMessageReceived += HandleServerMessage;
        
        // Connect to server
        LoginButton.IsEnabled = false;
        bool connected = await _tcpClient.ConnectAsync();
        
        if (!connected)
        {
            ShowError("Gagal terhubung ke server. Pastikan server sudah running.");
        }
    }
    
    private void OnGuestModeChanged(object sender, RoutedEventArgs e)
    {
        bool isGuestMode = GuestModeCheckbox.IsChecked ?? false;
        
        UsernameInput.IsEnabled = !isGuestMode;
        PasswordInput.IsEnabled = !isGuestMode;
        
        if (isGuestMode)
        {
            UsernameInput.Text = "";
            PasswordInput.Password = "";
            LoginButton.Content = "🎮 MULAI SEBAGAI GUEST";
        }
        else
        {
            LoginButton.Content = "🔓 LOGIN";
            UsernameInput.Focus();
        }
    }
    
    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        if (!_isConnected)
        {
            ShowError("Tidak terhubung ke server!");
            return;
        }
        
        bool isGuestMode = GuestModeCheckbox.IsChecked ?? false;
        string username = UsernameInput.Text.Trim();
        string password = PasswordInput.Password;
        
        // Validation
        if (!isGuestMode && string.IsNullOrEmpty(username))
        {
            ShowError("Username tidak boleh kosong!");
            UsernameInput.Focus();
            return;
        }
        
        // Disable button while processing
        LoginButton.IsEnabled = false;
        LoginButton.Content = "⏳ Memproses...";
        HideError();
        
        try
        {
            // Send login request to server
            await _tcpClient.SendMessageAsync(new Message
            {
                Type = "CLIENT_LOGIN",
                Data = new
                {
                    client_id = _config.Client.Name,
                    username = isGuestMode ? "GUEST" : username,
                    password = password,
                    is_guest = isGuestMode,
                    ip_address = NetworkHelper.GetLocalIPAddress()
                }
            });
            
            Logger.Info($"Login request sent: {(isGuestMode ? "GUEST" : username)}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Login failed: {ex.Message}");
            ShowError("Gagal mengirim request login.");
            LoginButton.IsEnabled = true;
            LoginButton.Content = isGuestMode ? "🎮 MULAI SEBAGAI GUEST" : "🔓 LOGIN";
        }
    }
    
    private void HandleServerMessage(Message message)
    {
        Dispatcher.Invoke(() =>
        {
            switch (message.Type)
            {
                case "LOGIN_SUCCESS":
                    OnLoginSuccess(message);
                    break;
                    
                case "LOGIN_FAILED":
                    OnLoginFailed(message);
                    break;
                    
                case MessageType.UNLOCK:
                    // Server unlocked the PC, proceed to main screen
                    OpenMainScreen();
                    break;
            }
        });
    }
    
    private void OnLoginSuccess(Message message)
    {
        Logger.Info("Login successful");
        
        var data = JObject.FromObject(message.Data ?? new { });
        string welcomeMessage = data["message"]?.Value<string>() ?? "Login berhasil!";
        
        MessageBox.Show(welcomeMessage, "Login Berhasil", 
            MessageBoxButton.OK, MessageBoxImage.Information);
        
        // Wait for UNLOCK message from server or show waiting screen
        ShowInfo("Menunggu operator untuk memulai sesi...");
        LoginButton.Content = "⏳ Menunggu operator...";
    }
    
    private void OnLoginFailed(Message message)
    {
        var data = JObject.FromObject(message.Data ?? new { });
        string errorMessage = data["message"]?.Value<string>() ?? "Login gagal!";
        
        Logger.Warning($"Login failed: {errorMessage}");
        ShowError(errorMessage);
        
        LoginButton.IsEnabled = true;
        LoginButton.Content = (GuestModeCheckbox.IsChecked ?? false) 
            ? "🎮 MULAI SEBAGAI GUEST" 
            : "🔓 LOGIN";
        
        UsernameInput.Focus();
    }
    
    private void OpenMainScreen()
    {
        // Close login screen and show lock screen or timer
        var lockScreen = new LockScreen();
        lockScreen.Show();
        
        this.Close();
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnLoginClick(sender, e);
        }
    }
    
    private void ShowError(string message)
    {
        StatusText.Text = $"❌ {message}";
        StatusText.Foreground = System.Windows.Media.Brushes.Red;
        StatusText.Visibility = Visibility.Visible;
    }
    
    private void ShowInfo(string message)
    {
        StatusText.Text = $"ℹ️ {message}";
        StatusText.Foreground = System.Windows.Media.Brushes.Orange;
        StatusText.Visibility = Visibility.Visible;
    }
    
    private void HideError()
    {
        StatusText.Visibility = Visibility.Collapsed;
    }
}

// Config classes (if not already defined in LockScreen.xaml.cs)
public class ClientConfig
{
    public ServerSettings Server { get; set; } = new();
    public ClientSettings Client { get; set; } = new();
}

public class ServerSettings
{
    public string Ip { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9999;
}

public class ClientSettings
{
    public string Name { get; set; } = "PC-01";
}