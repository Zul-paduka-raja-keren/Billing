namespace Shared.Protocol;

public class Message
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
}