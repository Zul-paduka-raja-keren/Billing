namespace WebDashboard.Services;

using Microsoft.AspNetCore.SignalR;

public class BillingHub : Hub
{
    public async Task SendClientUpdate(string clientId, object data)
    {
        await Clients.All.SendAsync("ReceiveClientUpdate", clientId, data);
    }
    
    public async Task RequestStartSession(string clientId, int minutes)
    {
        await Clients.All.SendAsync("StartSessionRequest", clientId, minutes);
    }
    
    public async Task BroadcastRevenue(decimal amount)
    {
        await Clients.All.SendAsync("RevenueUpdate", amount);
    }
}