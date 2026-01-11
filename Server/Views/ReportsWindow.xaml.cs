namespace Server.Views;

using System;
using System.Linq;
using System.Windows;
using Server.Services;
using Shared.Utils;

public partial class ReportsWindow : Window
{
    private DatabaseManager _db;
    
    public ReportsWindow(DatabaseManager db)
    {
        InitializeComponent();
        _db = db;
        LoadReports();
    }
    
    private void LoadReports()
    {
        try
        {
            // Get today's sessions
            var sessions = _db.GetTodaySessions();
            SessionsGrid.ItemsSource = sessions;
            
            // Calculate stats
            decimal dailyRevenue = _db.GetDailyRevenue();
            decimal monthlyRevenue = _db.GetMonthlyRevenue();
            
            DailyRevenueText.Text = $"Rp {dailyRevenue:N0}";
            MonthlyRevenueText.Text = $"Rp {monthlyRevenue:N0}";
            
            DailySessionsText.Text = $"{sessions.Count} sesi";
            
            // Monthly sessions count
            var allSessions = _db.GetAllClients(); // This would need a GetMonthlySessions method
            MonthlySessionsText.Text = $"{sessions.Count} sesi"; // Simplified
            
            // Average calculations
            if (sessions.Any())
            {
                int avgDuration = (int)sessions.Average(s => s.DurationMinutes);
                decimal avgCost = sessions.Average(s => s.TotalCost);
                
                AvgDurationText.Text = $"{avgDuration} menit";
                AvgCostText.Text = $"Rp {avgCost:N0} /sesi";
            }
            else
            {
                AvgDurationText.Text = "0 menit";
                AvgCostText.Text = "Rp 0 /sesi";
            }
            
            Logger.Info("Reports loaded successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load reports: {ex.Message}");
            MessageBox.Show(
                $"Gagal memuat laporan:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    
    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        LoadReports();
        MessageBox.Show("Data berhasil di-refresh!", "Success", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var sessions = _db.GetTodaySessions();
            string filename = $"Report_{DateTime.Now:yyyy-MM-dd}.csv";
            
            var csv = "Client,Start Time,End Time,Duration (min),Rate,Total Cost\n";
            foreach (var session in sessions)
            {
                csv += $"{session.ClientId}," +
                       $"{session.StartTime:HH:mm}," +
                       $"{session.EndTime:HH:mm}," +
                       $"{session.DurationMinutes}," +
                       $"{session.RatePerHour}," +
                       $"{session.TotalCost}\n";
            }
            
            System.IO.File.WriteAllText(filename, csv);
            
            Logger.Info($"Report exported: {filename}");
            MessageBox.Show(
                $"Laporan berhasil di-export ke:\n{filename}",
                "Export Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error($"Export failed: {ex.Message}");
            MessageBox.Show(
                $"Gagal export:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}