namespace Server.Helpers;

public static class TimeFormatter
{
    public static string FormatDuration(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
    
    public static string FormatShortDuration(int totalMinutes)
    {
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        
        if (hours > 0)
            return $"{hours}j {minutes}m";
        return $"{minutes}m";
    }
}