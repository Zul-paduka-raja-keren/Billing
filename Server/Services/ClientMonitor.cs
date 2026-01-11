namespace Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Shared.Utils;

public class ClientMonitor
{
    private List<MonitoredClient> _clients = new();
    private Timer _checkTimer;
    
    public event Action<MonitoredClient>? OnClientTimeout;
    
    public ClientMonitor()
    {
        _checkTimer = new Timer(30000); // Check every 30 seconds
        _checkTimer.Elapsed += CheckClientsHealth;
    }
    
    public void Start()
    {
        _checkTimer.Start();
        Logger.Info("Client monitor started");
    }
    
    public void Stop()
    {
        _checkTimer.Stop();
    }
    
    public void AddClient(string clientId, string name, string ipAddress)
    {
        var client = new MonitoredClient
        {
            Id = clientId,
            Name = name,
            IpAddress = ipAddress,
            LastSeen = DateTime.Now,
            IsHealthy = true
        };
        
        _clients.Add(client);
        Logger.Info($"Client added to monitor: {name}");
    }
    
    public void RemoveClient(string clientId)
    {
        _clients.RemoveAll(c => c.Id == clientId);
        Logger.Info($"Client removed from monitor: {clientId}");
    }
    
    public void UpdateHeartbeat(string clientId)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client != null)
        {
            client.LastSeen = DateTime.Now;
            client.IsHealthy = true;
        }
    }
    
    public MonitoredClient? GetClient(string clientId)
    {
        return _clients.FirstOrDefault(c => c.Id == clientId);
    }
    
    public IEnumerable<MonitoredClient> GetAllClients()
    {
        return _clients;
    }
    
    public int GetActiveCount()
    {
        return _clients.Count(c => c.IsHealthy);
    }
    
    private void CheckClientsHealth(object? sender, ElapsedEventArgs e)
    {
        var timeout = TimeSpan.FromSeconds(60); // 60 seconds timeout
        
        foreach (var client in _clients.ToList())
        {
            if (DateTime.Now - client.LastSeen > timeout && client.IsHealthy)
            {
                client.IsHealthy = false;
                Logger.Warning($"Client timeout: {client.Name}");
                OnClientTimeout?.Invoke(client);
            }
        }
    }
}

public class MonitoredClient
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public DateTime LastSeen { get; set; }
    public bool IsHealthy { get; set; }
}