namespace Server.ViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;

public class MainViewModel : INotifyPropertyChanged
{
    private decimal _dailyRevenue;
    private decimal _monthlyRevenue;
    private int _activeSessions;
    
    public ObservableCollection<ClientViewModel> Clients { get; set; } = new();
    
    public decimal DailyRevenue
    {
        get => _dailyRevenue;
        set { _dailyRevenue = value; OnPropertyChanged(nameof(DailyRevenue)); }
    }
    
    public decimal MonthlyRevenue
    {
        get => _monthlyRevenue;
        set { _monthlyRevenue = value; OnPropertyChanged(nameof(MonthlyRevenue)); }
    }
    
    public int ActiveSessions
    {
        get => _activeSessions;
        set { _activeSessions = value; OnPropertyChanged(nameof(ActiveSessions)); }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}