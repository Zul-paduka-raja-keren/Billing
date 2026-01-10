namespace Shared.Utils;

public static class DateTimeHelper
{
    public static string FormatDuration(int seconds)
    {
        var span = TimeSpan.FromSeconds(seconds);
        return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
    }
    
    public static string FormatTime(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }
    
    public static string FormatDate(DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy");
    }
}