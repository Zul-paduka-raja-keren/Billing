namespace Server.Services;

using System.Collections.Generic;
using System.Linq;
using Shared.Models;
using Shared.Utils;

public class SessionManager
{
    private DatabaseManager _db;
    private Dictionary<string, Session> _activeSessions = new();
    
    public SessionManager(DatabaseManager db)
    {
        _db = db;
    }
    
    public Session? GetActiveSession(string clientId)
    {
        return _activeSessions.ContainsKey(clientId) ? _activeSessions[clientId] : null;
    }
    
    public void StartSession(Session session)
    {
        _activeSessions[session.ClientId] = session;
        Logger.Info($"Session started: {session.ClientId}");
    }
    
    public void EndSession(string clientId)
    {
        if (_activeSessions.ContainsKey(clientId))
        {
            _activeSessions.Remove(clientId);
            Logger.Info($"Session ended: {clientId}");
        }
    }
    
    public IEnumerable<Session> GetAllActiveSessions()
    {
        return _activeSessions.Values;
    }
    
    public int GetActiveSessionCount()
    {
        return _activeSessions.Count;
    }
}