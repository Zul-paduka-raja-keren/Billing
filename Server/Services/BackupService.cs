namespace Server.Services;

using System;
using System.IO;
using System.Timers;
using Shared.Utils;

public class BackupService
{
    private Timer _timer;
    private string _dbPath;
    private string _backupPath;
    
    public BackupService(string dbPath, string backupPath, int intervalHours = 24)
    {
        _dbPath = dbPath;
        _backupPath = backupPath;
        
        Directory.CreateDirectory(_backupPath);
        
        _timer = new Timer(intervalHours * 3600 * 1000);
        _timer.Elapsed += PerformBackup;
    }
    
    public void Start()
    {
        _timer.Start();
        Logger.Info("Backup service started");
        
        // Immediate backup on start
        PerformBackup(null, null);
    }
    
    public void Stop()
    {
        _timer.Stop();
    }
    
    private void PerformBackup(object? sender, ElapsedEventArgs? e)
    {
        try
        {
            string backupFile = Path.Combine(
                _backupPath, 
                $"billing_{DateTime.Now:yyyy-MM-dd_HHmmss}.db");
            
            File.Copy(_dbPath, backupFile, overwrite: true);
            
            Logger.Info($"Database backed up to: {backupFile}");
            
            // Delete old backups (keep last 7 days)
            CleanOldBackups();
        }
        catch (Exception ex)
        {
            Logger.Error($"Backup failed: {ex.Message}");
        }
    }
    
    private void CleanOldBackups()
    {
        try
        {
            var files = Directory.GetFiles(_backupPath, "billing_*.db");
            var cutoffDate = DateTime.Now.AddDays(-7);
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                    Logger.Info($"Deleted old backup: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Cleanup failed: {ex.Message}");
        }
    }
}