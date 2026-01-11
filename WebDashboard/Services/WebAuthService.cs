namespace WebDashboard.Services;

public class WebAuthService
{
    private bool _isAuthenticated = false;
    private string _currentUser = "";
    
    public bool Login(string username, string password)
    {
        // Simple auth for demo
        if (username == "admin" && password == "admin")
        {
            _isAuthenticated = true;
            _currentUser = username;
            return true;
        }
        return false;
    }
    
    public bool IsAuthenticated() => _isAuthenticated;
    
    public string GetCurrentUser() => _currentUser;
    
    public void Logout()
    {
        _isAuthenticated = false;
        _currentUser = "";
    }
}