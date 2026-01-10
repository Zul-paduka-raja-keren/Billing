namespace Client.Views;

using System;
using System.Windows;
using System.Windows.Input;
using Client.Services;
using Shared.Protocol;
using Shared.Utils;
using Newtonsoft.Json;

public partial class LockScreen : Window
{
    private TcpClientService _tcpClient;
    private HeartbeatService _heartbeat;
    private IdleDetector _idleDetector;
    private ProcessMonitor? _processMonitor;
    private ClientConfig _config;
    
    public LockScreen()
    {
        InitializeComponent();
        LoadConfig();
        InitializeServices();
        
        // Block close button
        this.Closing += (s, e) => e.Cancel = true;
        
        // Block keyboard shortcuts
        this.PreviewKeyDown += OnPreviewKeyDown;
        
        Logger.Info("LockScreen initialized");
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
    
    private void InitializeServices()
    {
        // TCP Client
        _tcpClient = new TcpClientService(_config.Server.Ip, _config.Server.Port);
        _tcpClient.OnMessageReceived += HandleServerMessage;
        _tcpClient.OnConnected += () => UpdateStatus("Terhubung ke server");
        _tcpClient.OnDisconnected += () => UpdateStatus("Terputus dari server");
        
        // Heartbeat
        _heartbeat = new HeartbeatService(_tcpClient);
        
        // Idle Detector
        _idleDetector = new IdleDetector(10);
        _idleDetector.OnIdleDetected += async () =>
        {
            await _tcpClient.SendMessageAsync(new Message
            {
                Type = MessageType.IDLE_DETECTED,
                Data = new { idle_duration = 600 }
            });
        };
        
        // Process Monitor (if enabled)
        if (_config.Features.EnableProcessMonitor)
        {
            _processMonitor = new ProcessMonitor(new[] { "cheatengine", "processhacker" });
            _processMonitor.Start();
        }
        
        // Connect to server
        _ = ConnectToServer();
    }
    
    private async System.Threading.Tasks.Task ConnectToServer()
    {
        while (!_tcpClient.IsConnected)
        {
            UpdateStatus("Menghubungkan ke server...");
            bool connected = await _tcpClient.ConnectAsync();
            
            if (connected)
            {
                _heartbeat.Start();
                _idleDetector.Start();
                
                // Send client info
                await _tcpClient.SendMessageAsync(new Message
                {
                    Type = MessageType.CLIENT_INFO,
                    Data = new
                    {
                        name = _config.Client.Name,
                        ip = NetworkHelper.GetLocalIPAddress()
                    }
                });
            }
            else
            {
                await System.Threading.Tasks.Task.Delay(5000);
            }
        }
    }
    
    private void HandleServerMessage(Message message)
    {
        Dispatcher.Invoke(() =>
        {
            switch (message.Type)
            {
                case MessageType.UNLOCK:
                    UnlockScreen(message);
                    break;
                    
                case MessageType.LOCK:
                    LockScreenNow();
                    break;
                    
                case MessageType.TIME_UPDATE:
                    UpdateTimeDisplay(message);
                    break;
                    
                case MessageType.SHUTDOWN:
                    ShutdownPC();
                    break;
            }
        });
    }
    
    private void UnlockScreen(Message message)
    {
        Logger.Info("Screen unlocked");
        
        // Hide lock screen and show timer
        this.Hide();
        
        var timerWindow = new TimerDisplay(_tcpClient);
        timerWindow.Show();
        
        // Close this window
        this.Closing -= (s, e) => e.Cancel = true;
        this.Close();
    }
    
    private void LockScreenNow()
    {
        Logger.Info("Screen locked");
        this.Show();
        this.Topmost = true;
        this.WindowState = WindowState.Maximized;
    }
    
    private void UpdateTimeDisplay(Message message)
    {
        // Will be handled by TimerDisplay window
    }
    
    private void ShutdownPC()
    {
        Logger.Warning("Shutdown command received");
        System.Diagnostics.Process.Start("shutdown", "/s /t 0");
    }
    
    private void UpdateStatus(string status)
    {
        StatusText.Text = $"Status: {status}";
    }
    
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Block Alt+F4, Ctrl+Alt+Del, etc
        if ((e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt) ||
            (e.Key == Key.Delete && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt)))
        {
            e.Handled = true;
        }
    }
}

// Config classes
public class ClientConfig
{
    public ServerSettings Server { get; set; } = new();
    public ClientSettings Client { get; set; } = new();
    public FeaturesSettings Features { get; set; } = new();
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

public class FeaturesSettings
{
    public bool EnableProcessMonitor { get; set; } = true;
}