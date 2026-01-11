namespace Server.Helpers;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public class NetworkScanner
{
    public static async Task<List<string>> ScanLocalNetwork()
    {
        var activeIPs = new List<string>();
        var localIP = GetLocalIPAddress();
        
        if (string.IsNullOrEmpty(localIP)) return activeIPs;
        
        var subnet = localIP.Substring(0, localIP.LastIndexOf('.'));
        var tasks = new List<Task>();
        
        for (int i = 1; i < 255; i++)
        {
            string ip = $"{subnet}.{i}";
            tasks.Add(Task.Run(async () =>
            {
                if (await PingHost(ip))
                {
                    lock (activeIPs)
                    {
                        activeIPs.Add(ip);
                    }
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        return activeIPs;
    }
    
    private static async Task<bool> PingHost(string host)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, 1000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
    
    private static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return string.Empty;
    }
}