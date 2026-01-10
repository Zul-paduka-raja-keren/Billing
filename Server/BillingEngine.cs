using System;
using System.Timers;
using System.Collections.Generic;

public class BillingEngine
{
    private Dictionary<string, ActiveSession> _sessions = new Dictionary<string, ActiveSession>();
    private Timer _ticker;
    private TcpServer _server;
    
    public event Action<ActiveSession> OnSessionExpired;
    
    public BillingEngine(TcpServer server)
    {
        _server = server;
        _ticker = new Timer(1000); // Tiap detik
        _ticker.Elapsed += Tick;
        _ticker.Start();
    }
    
    public void StartSession(string clientId, int minutes, decimal rate)
    {
        var session = new ActiveSession
        {
            ClientId = clientId,
            StartTime = DateTime.Now,
            RemainingSeconds = minutes * 60,
            RatePerHour = rate,
            IsPaused = false
        };
        
        _sessions[clientId] = session;
        
        // Unlock client
        _server.SendTo(clientId, new Message 
        { 
            Type = "UNLOCK",
            Data = new { remaining = session.RemainingSeconds }
        });
    }
    
    public void PauseSession(string clientId)
    {
        if (_sessions.ContainsKey(clientId))
        {
            _sessions[clientId].IsPaused = true;
            _server.SendTo(clientId, new Message { Type = "LOCK" });
        }
    }
    
    public void ExtendTime(string clientId, int additionalMinutes)
    {
        if (_sessions.ContainsKey(clientId))
        {
            _sessions[clientId].RemainingSeconds += additionalMinutes * 60;
        }
    }
    
    private void Tick(object sender, ElapsedEventArgs e)
    {
        foreach (var session in _sessions.Values)
        {
            if (session.IsPaused) continue;
            
            session.RemainingSeconds--;
            
            // Update client setiap 5 detik (hemat bandwidth)
            if (session.RemainingSeconds % 5 == 0)
            {
                _server.SendTo(session.ClientId, new Message
                {
                    Type = "TIME_UPDATE",
                    Data = new { remaining = session.RemainingSeconds }
                });
            }
            
            // Habis waktu
            if (session.RemainingSeconds <= 0)
            {
                _server.SendTo(session.ClientId, new Message { Type = "LOCK" });
                OnSessionExpired?.Invoke(session);
                _sessions.Remove(session.ClientId);
            }
        }
    }
}

public class ActiveSession
{
    public string ClientId { get; set; }
    public DateTime StartTime { get; set; }
    public int RemainingSeconds { get; set; }
    public decimal RatePerHour { get; set; }
    public bool IsPaused { get; set; }
    
    public decimal TotalCost => 
        (decimal)(DateTime.Now - StartTime).TotalHours * RatePerHour;
}