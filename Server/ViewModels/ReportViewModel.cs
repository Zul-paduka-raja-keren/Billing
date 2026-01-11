namespace Server.ViewModels;

public class ReportViewModel
{
    public decimal DailyRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    
    public int TotalSessions { get; set; }
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    
    public decimal AverageSessionCost { get; set; }
    public int AverageSessionDuration { get; set; }
    
    public string DisplayDailyRevenue => $"Rp {DailyRevenue:N0}";
    public string DisplayMonthlyRevenue => $"Rp {MonthlyRevenue:N0}";
}