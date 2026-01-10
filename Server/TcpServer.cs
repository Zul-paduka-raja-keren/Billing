using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TcpServer
{
    private TcpListener _listener;
    private List<ClientConnection> _clients = new List<ClientConnection>();
    
    public event Action<string, Message> OnMessageReceived;
    
    public void Start(int port = 9999)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Console.WriteLine($"[SERVER] Listening on port {port}");
        
        Task.Run(() => AcceptClients());
    }
    
    private async void AcceptClients()
    {
        while (true)
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            var client = new ClientConnection(tcpClient, this);
            _clients.Add(client);
            Console.WriteLine($"[CLIENT] Connected: {client.Id}");
        }
    }
    
    public void SendTo(string clientId, Message msg)
    {
        var client = _clients.Find(c => c.Id == clientId);
        client?.Send(msg);
    }
    
    public void Broadcast(Message msg)
    {
        foreach (var c in _clients)
            c.Send(msg);
    }
    
    internal void HandleMessage(ClientConnection client, Message msg)
    {
        OnMessageReceived?.Invoke(client.Id, msg);
    }
}

public class ClientConnection
{
    public string Id { get; }
    private TcpClient _tcp;
    private NetworkStream _stream;
    private TcpServer _server;
    
    public ClientConnection(TcpClient tcp, TcpServer server)
    {
        _tcp = tcp;
        _server = server;
        _stream = tcp.GetStream();
        Id = Guid.NewGuid().ToString().Substring(0, 8);
        
        Task.Run(() => Listen());
    }
    
    private async void Listen()
    {
        byte[] buffer = new byte[4096];
        while (true)
        {
            try
            {
                int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes == 0) break; // Disconnected
                
                string json = Encoding.UTF8.GetString(buffer, 0, bytes);
                var msg = JsonConvert.DeserializeObject<Message>(json);
                _server.HandleMessage(this, msg);
            }
            catch { break; }
        }
    }
    
    public void Send(Message msg)
    {
        try
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);
            _stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Send failed: {ex.Message}");
        }
    }
}