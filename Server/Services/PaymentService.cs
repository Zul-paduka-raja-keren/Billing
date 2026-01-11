namespace Server.Services;

using System;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Utils;

public class PaymentService
{
    private DatabaseManager _db;
    
    public PaymentService(DatabaseManager db)
    {
        _db = db;
    }
    
    public async Task<Payment> CreatePaymentAsync(string clientId, decimal amount, string method)
    {
        var payment = new Payment
        {
            OrderId = GenerateOrderId(),
            ClientId = clientId,
            Amount = amount,
            Method = method,
            Status = "pending",
            CreatedAt = DateTime.Now
        };
        
        Logger.Info($"Payment created: {payment.OrderId} - Rp {amount:N0} ({method})");
        return payment;
    }
    
    public async Task<bool> ProcessCashPaymentAsync(Payment payment)
    {
        payment.Status = "success";
        payment.CompletedAt = DateTime.Now;
        
        Logger.Info($"Cash payment completed: {payment.OrderId}");
        return true;
    }
    
    public async Task<string> GenerateQRISAsync(Payment payment)
    {
        // Integration with Midtrans QRIS API
        // This is a placeholder implementation
        
        Logger.Info($"QRIS generated for: {payment.OrderId}");
        return "https://api.midtrans.com/qr/..." + payment.OrderId;
    }
    
    public async Task<string> GenerateEWalletLinkAsync(Payment payment, string provider)
    {
        // Generate deep link for e-wallet (GoPay/OVO)
        
        Logger.Info($"{provider} link generated for: {payment.OrderId}");
        return $"{provider.ToLower()}://payment?order_id={payment.OrderId}&amount={payment.Amount}";
    }
    
    private string GenerateOrderId()
    {
        return $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
    }
}