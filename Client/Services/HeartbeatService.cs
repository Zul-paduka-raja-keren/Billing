namespace Client.Services;

using System;
using System.Threading.Tasks;
using System.Timers;
using Shared.Protocol;

public class HeartbeatService
{
    private Timer _timer;
    private TcpClientService _tcpClient;
    
    public HeartbeatService(TcpClientService tcpClient)
    {
        _tcpClient = tcpClient;
        _timer = new Timer(10000); // Every 10 seconds
        _timer.Elapsed += SendHeartbeat;
    }
    
    public void Start()
    {
        _timer.Start();
    }
    
    public void Stop()
    {
        _timer.Stop();
    }
    
    private async void SendHeartbeat(object? sender, ElapsedEventArgs e)
    {
        if (!_tcpClient.IsConnected) return;
        
        await _tcpClient.SendMessageAsync(new Message
        {
            Type = MessageType.HEARTBEAT,
            Data = new
            {
                timestamp = DateTime.Now,
                status = "alive"
            }
        });
    }
}