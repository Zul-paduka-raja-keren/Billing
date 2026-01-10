namespace Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Shared.Protocol;
using Shared.Utils;

public class TcpServerService
{
    private TcpListener? _listener;
    private List<ConnectedClient> _clients = new();
    private bool _isRunning;
    
    public event Action<ConnectedClient, Message>? OnMessageReceived;
    public event Action<ConnectedClient>? OnClientConnected;
    public event Action<ConnectedClient>? OnClientDisconnected;
    
    public IReadOnlyList<ConnectedClient> Clients => _clients.AsReadOnly();
    
    public void Start(int port = 9999)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _isRunning = true;
        
        Logger.Info($"TCP Server started on port {port}");
        
        Task.Run(AcceptClients);
    }
    
    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        
        foreach (var client in _clients.ToList())
        {
            client.Disconnect();
        }
        
        Logger.Info("TCP Server stopped");
    }
    
    private async Task AcceptClients()
    {
        while (_isRunning)
        {
            try
            {
                var tcpClient = await _listener!.AcceptTcpClientAsync();
                var client = new ConnectedClient(tcpClient, this);
                
                _clients.Add(client);
                Logger.Info($"Client connected: {client.Id}");
                
                OnClientConnected?.Invoke(client);
            }
            catch (Exception ex)
            {
                if (_isRunning)
                    Logger.Error($"Accept client error: {ex.Message}");
            }
        }
    }
    
    public async Task SendToAsync(string clientId, Message message)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        await client?.SendAsync(message)!;
    }
    
    public async Task BroadcastAsync(Message message)
    {
        foreach (var client in _clients.ToList())
        {
            await client.SendAsync(message);
        }
    }
    
    internal void HandleMessage(ConnectedClient client, Message message)
    {
        OnMessageReceived?.Invoke(client, message);
    }
    
    internal void HandleDisconnection(ConnectedClient client)
    {
        _clients.Remove(client);
        Logger.Info($"Client disconnected: {client.Id}");
        OnClientDisconnected?.Invoke(client);
    }
}

public class ConnectedClient
{
    public string Id { get; }
    public string? Name { get; set; }
    public string IpAddress { get; }
    public DateTime ConnectedAt { get; }
    
    private TcpClient _tcp;
    private NetworkStream _stream;
    private TcpServerService _server;
    private Task? _listenerTask;
    
    public ConnectedClient(TcpClient tcp, TcpServerService server)
    {
        _tcp = tcp;
        _server = server;
        _stream = tcp.GetStream();
        
        Id = Guid.NewGuid().ToString().Substring(0, 8);
        IpAddress = ((IPEndPoint)tcp.Client.RemoteEndPoint!).Address.ToString();
        ConnectedAt = DateTime.Now;
        
        _listenerTask = Task.Run(ListenForMessages);
    }
    
    public async Task SendAsync(Message message)
    {
        try
        {
            await NetworkHelper.SendMessageAsync(_stream, message);
        }
        catch (Exception ex)
        {
            Logger.Error($"Send to {Id} failed: {ex.Message}");
        }
    }
    
    private async Task ListenForMessages()
    {
        while (_tcp.Connected)
        {
            try
            {
                var message = await NetworkHelper.ReceiveMessageAsync(_stream);
                
                if (message == null)
                {
                    Disconnect();
                    break;
                }
                
                _server.HandleMessage(this, message);
            }
            catch
            {
                Disconnect();
                break;
            }
        }
    }
    
    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _tcp?.Close();
            _server.HandleDisconnection(this);
        }
        catch { }
    }
}