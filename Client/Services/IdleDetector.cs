namespace Client.Services;

using System;
using System.Runtime.InteropServices;
using System.Timers;
using Shared.Utils;

public class IdleDetector
{
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    
    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
    
    private Timer _timer;
    private int _idleThresholdMinutes;
    
    public event Action? OnIdleDetected;
    public bool IsIdle { get; private set; }
    
    public IdleDetector(int idleThresholdMinutes = 10)
    {
        _idleThresholdMinutes = idleThresholdMinutes;
        _timer = new Timer(30000); // Check every 30 seconds
        _timer.Elapsed += CheckIdle;
    }
    
    public void Start()
    {
        _timer.Start();
        Logger.Info("Idle detector started");
    }
    
    public void Stop()
    {
        _timer.Stop();
    }
    
    private void CheckIdle(object? sender, ElapsedEventArgs e)
    {
        int idleSeconds = GetIdleTimeSeconds();
        int thresholdSeconds = _idleThresholdMinutes * 60;
        
        if (idleSeconds >= thresholdSeconds && !IsIdle)
        {
            IsIdle = true;
            Logger.Warning($"User idle detected: {idleSeconds}s");
            OnIdleDetected?.Invoke();
        }
        else if (idleSeconds < 5 && IsIdle)
        {
            IsIdle = false;
            Logger.Info("User activity resumed");
        }
    }
    
    private int GetIdleTimeSeconds()
    {
        LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
        lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
        
        if (GetLastInputInfo(ref lastInputInfo))
        {
            uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
            return (int)(idleTime / 1000);
        }
        
        return 0;
    }
}