namespace Client.Services;

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Shared.Protocol;
using Shared.Utils;

public class TcpClientService
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private string _serverIp;
    private int _serverPort;
    private bool _isConnected;
    private Task? _listenerTask;
    
    public event Action<Message>? OnMessageReceived;
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    
    public bool IsConnected => _isConnected;
    
    public TcpClientService(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
    }
    
    public async Task<bool> ConnectAsync()
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_serverIp, _serverPort);
            _stream = _client.GetStream();
            _isConnected = true;
            
            Logger.Info($"Connected to server {_serverIp}:{_serverPort}");
            OnConnected?.Invoke();
            
            // Start listening for messages
            _listenerTask = Task.Run(ListenForMessages);
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Connection failed: {ex.Message}");
            return false;
        }
    }
    
    public async Task DisconnectAsync()
    {
        _isConnected = false;
        _stream?.Close();
        _client?.Close();
        OnDisconnected?.Invoke();
        Logger.Info("Disconnected from server");
    }
    
    public async Task SendMessageAsync(Message message)
    {
        if (!_isConnected || _stream == null) return;
        
        try
        {
            await NetworkHelper.SendMessageAsync(_stream, message);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to send message: {ex.Message}");
            await HandleDisconnection();
        }
    }
    
    private async Task ListenForMessages()
    {
        while (_isConnected && _stream != null)
        {
            try
            {
                var message = await NetworkHelper.ReceiveMessageAsync(_stream);
                
                if (message == null)
                {
                    await HandleDisconnection();
                    break;
                }
                
                OnMessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                Logger.Error($"Listener error: {ex.Message}");
                await HandleDisconnection();
                break;
            }
        }
    }
    
    private async Task HandleDisconnection()
    {
        if (!_isConnected) return;
        
        _isConnected = false;
        Logger.Warning("Server disconnected");
        OnDisconnected?.Invoke();
        
        // Auto-reconnect after 5 seconds
        await Task.Delay(5000);
        await ConnectAsync();
    }
}