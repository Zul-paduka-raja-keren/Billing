namespace Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Shared.Protocol;
using Shared.Models;
using Shared.Utils;

public class BillingEngine
{
    private Dictionary<string, ActiveSession> _sessions = new();
    private System.Timers.Timer _ticker;
    private TcpServerService _server;
    private DatabaseManager _db;
    
    public event Action<ActiveSession>? OnSessionExpired;
    public event Action<ActiveSession>? OnSessionWarning;
    
    public IReadOnlyDictionary<string, ActiveSession> ActiveSessions => _sessions;
    
    public BillingEngine(TcpServerService server, DatabaseManager db)
    {
        _server = server;
        _db = db;
        _ticker = new System.Timers.Timer(1000); // Every second
        _ticker.Elapsed += Tick;
    }
    
    public void Start()
    {
        _ticker.Start();
        Logger.Info("Billing engine started");
    }
    
    public void Stop()
    {
        _ticker.Stop();
        Logger.Info("Billing engine stopped");
    }
    
    public void StartSession(string clientId, string clientName, int minutes, decimal ratePerHour)
    {
        var session = new ActiveSession
        {
            ClientId = clientId,
            ClientName = clientName,
            StartTime = DateTime.Now,
            RemainingSeconds = minutes * 60,
            RatePerHour = ratePerHour,
            IsPaused = false
        };
        
        _sessions[clientId] = session;
        
        // Save to database
        session.DbId = _db.SaveSession(new Session
        {
            ClientId = clientId,
            StartTime = session.StartTime,
            RatePerHour = ratePerHour,
            Status = "active"
        });
        
        // Unlock client
        _ = _server.SendToAsync(clientId, new Message
        {
            Type = MessageType.UNLOCK,
            Data = new { remaining = session.RemainingSeconds }
        });
        
        Logger.Info($"Session started for {clientName} ({minutes} minutes)");
    }
    
    public void PauseSession(string clientId)
    {
        if (!_sessions.ContainsKey(clientId)) return;
        
        _sessions[clientId].IsPaused = true;
        _ = _server.SendToAsync(clientId, new Message { Type = MessageType.LOCK });
        
        Logger.Info($"Session paused for {clientId}");
    }
    
    public void ResumeSession(string clientId)
    {
        if (!_sessions.ContainsKey(clientId)) return;
        
        _sessions[clientId].IsPaused = false;
        _ = _server.SendToAsync(clientId, new Message { Type = MessageType.UNLOCK });
        
        Logger.Info($"Session resumed for {clientId}");
    }
    
    public void ExtendTime(string clientId, int additionalMinutes)
    {
        if (!_sessions.ContainsKey(clientId)) return;
        
        _sessions[clientId].RemainingSeconds += additionalMinutes * 60;
        Logger.Info($"Extended {clientId} by {additionalMinutes} minutes");
    }
    
    public void EndSession(string clientId)
    {
        if (!_sessions.TryGetValue(clientId, out var session)) return;
        
        // Lock client
        _ = _server.SendToAsync(clientId, new Message { Type = MessageType.LOCK });
        
        // Save to database
        _db.UpdateSession(new Session
        {
            Id = session.DbId,
            ClientId = session.ClientId,
            StartTime = session.StartTime,
            EndTime = DateTime.Now,
            DurationMinutes = (int)(DateTime.Now - session.StartTime).TotalMinutes,
            TotalCost = session.TotalCost,
            Status = "completed"
        });
        
        _sessions.Remove(clientId);
        Logger.Info($"Session ended for {clientId}");
    }
    
    private void Tick(object? sender, ElapsedEventArgs e)
    {
        foreach (var session in _sessions.Values.ToList())
        {
            if (session.IsPaused) continue;
            
            session.RemainingSeconds--;
            
            // Update client every 5 seconds
            if (session.RemainingSeconds % 5 == 0)
            {
                _ = _server.SendToAsync(session.ClientId, new Message
                {
                    Type = MessageType.TIME_UPDATE,
                    Data = new { remaining = session.RemainingSeconds }
                });
            }
            
            // Warning at 5 minutes
            if (session.RemainingSeconds == 300)
            {
                OnSessionWarning?.Invoke(session);
            }
            
            // Time expired
            if (session.RemainingSeconds <= 0)
            {
                OnSessionExpired?.Invoke(session);
                EndSession(session.ClientId);
            }
        }
    }
}

public class ActiveSession
{
    public int DbId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int RemainingSeconds { get; set; }
    public decimal RatePerHour { get; set; }
    public bool IsPaused { get; set; }
    
    public decimal TotalCost => 
        (decimal)(DateTime.Now - StartTime).TotalHours * RatePerHour;
    
    public string RemainingTimeFormatted => 
        $"{RemainingSeconds / 3600:D2}:{(RemainingSeconds % 3600) / 60:D2}:{RemainingSeconds % 60:D2}";
}