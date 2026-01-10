namespace Shared.Utils;

public static class Logger
{
    private static readonly string LogPath = "Logs";
    
    public static void Info(string message, string source = "System")
    {
        Log("INFO", message, source);
    }
    
    public static void Warning(string message, string source = "System")
    {
        Log("WARNING", message, source);
    }
    
    public static void Error(string message, string source = "System")
    {
        Log("ERROR", message, source);
    }
    
    private static void Log(string level, string message, string source)
    {
        try
        {
            Directory.CreateDirectory(LogPath);
            string filename = Path.Combine(LogPath, $"app_{DateTime.Now:yyyy-MM-dd}.log");
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] [{source}] {message}";
            
            File.AppendAllText(filename, logEntry + Environment.NewLine);
            Console.WriteLine(logEntry);
        }
        catch { }
    }
}