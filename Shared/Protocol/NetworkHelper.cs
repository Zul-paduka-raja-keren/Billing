namespace Shared.Protocol;

using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public static class NetworkHelper
{
    public static async Task<Message?> ReceiveMessageAsync(NetworkStream stream)
    {
        try
        {
            byte[] buffer = new byte[8192];
            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            
            if (bytes == 0) return null;
            
            string json = Encoding.UTF8.GetString(buffer, 0, bytes);
            return JsonConvert.DeserializeObject<Message>(json);
        }
        catch
        {
            return null;
        }
    }
    
    public static async Task SendMessageAsync(NetworkStream stream, Message message)
    {
        try
        {
            string json = JsonConvert.SerializeObject(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Send failed: {ex.Message}");
        }
    }
    
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "127.0.0.1";
    }
}