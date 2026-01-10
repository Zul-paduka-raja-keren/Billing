namespace Client.Services;

using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Shared.Utils;

public class ProcessMonitor
{
    private Timer _timer;
    private string[] _blacklist;
    
    public ProcessMonitor(string[] blacklistedProcesses)
    {
        _blacklist = blacklistedProcesses.Select(p => p.ToLower()).ToArray();
        _timer = new Timer(5000); // Check every 5 seconds
        _timer.Elapsed += CheckProcesses;
    }
    
    public void Start()
    {
        _timer.Start();
        Logger.Info("Process monitor started");
    }
    
    public void Stop()
    {
        _timer.Stop();
    }
    
    private void CheckProcesses(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var processes = Process.GetProcesses();
            
            foreach (var process in processes)
            {
                try
                {
                    string processName = process.ProcessName.ToLower();
                    
                    if (_blacklist.Contains(processName))
                    {
                        Logger.Warning($"Killing blacklisted process: {processName}");
                        process.Kill();
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Process monitor error: {ex.Message}");
        }
    }
}