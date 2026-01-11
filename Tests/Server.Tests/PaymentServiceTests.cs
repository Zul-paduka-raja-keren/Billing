namespace Server.Tests;

using Xunit;
using Server.Services;

public class PaymentServiceTests
{
    [Fact]
    public async Task CreatePaymentAsync_ShouldGenerateOrderId()
    {
        // Arrange
        var db = new DatabaseManager(":memory:");
        var service = new PaymentService(db);
        
        // Act
        var payment = await service.CreatePaymentAsync("client1", 10000, "cash");
        
        // Assert
        Assert.NotEmpty(payment.OrderId);
        Assert.StartsWith("ORD-", payment.OrderId);
        Assert.Equal(10000, payment.Amount);
        Assert.Equal("cash", payment.Method);
    }
    
    [Fact]
    public async Task ProcessCashPaymentAsync_ShouldCompletePayment()
    {
        // Arrange
        var db = new DatabaseManager(":memory:");
        var service = new PaymentService(db);
        var payment = await service.CreatePaymentAsync("client1", 10000, "cash");
        
        // Act
        var result = await service.ProcessCashPaymentAsync(payment);
        
        // Assert
        Assert.True(result);
        Assert.Equal("success", payment.Status);
        Assert.NotNull(payment.CompletedAt);
    }
    
    [Fact]
    public async Task GenerateQRISAsync_ShouldReturnUrl()
    {
        // Arrange
        var db = new DatabaseManager(":memory:");
        var service = new PaymentService(db);
        var payment = await service.CreatePaymentAsync("client1", 10000, "qris");
        
        // Act
        var qrUrl = await service.GenerateQRISAsync(payment);
        
        // Assert
        Assert.NotEmpty(qrUrl);
        Assert.Contains("api.midtrans.com", qrUrl);
    }
}