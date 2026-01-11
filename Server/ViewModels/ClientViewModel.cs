namespace Server.ViewModels;

using System.ComponentModel;

public class ClientViewModel : INotifyPropertyChanged
{
    private string _status = "Idle";
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
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}