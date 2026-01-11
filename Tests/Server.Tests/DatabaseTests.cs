namespace Server.Tests;

using Xunit;
using Server.Services;
using Shared.Models;

public class DatabaseTests
{
    [Fact]
    public void InitializeDatabase_ShouldCreateTables()
    {
        // Arrange & Act
        var db = new DatabaseManager(":memory:");
        
        // Assert
        var clients = db.GetAllClients();
        Assert.NotNull(clients);
    }
    
    [Fact]
    public void GetDailyRevenue_ShouldReturnZeroInitially()
    {
        // Arrange
        var db = new DatabaseManager(":memory:");
        
        // Act
        var revenue = db.GetDailyRevenue();
        
        // Assert
        Assert.Equal(0, revenue);
    }
    
    [Fact]
    public void SaveSession_ShouldReturnId()
    {
        // Arrange
        var db = new DatabaseManager(":memory:");
        var session = new Session
        {
            ClientId = "test-client",
            StartTime = DateTime.Now,
            RatePerHour = 5000,
            Status = "active"
        };
        
        // Act
        int id = db.SaveSession(session);
        
        // Assert
        Assert.True(id > 0);
    }
    
    [Fact]
    public void GetActivePricing_ShouldReturnDefaultPricing()
    {
        // Arrange
        var db = new DatabaseManager(":memory:");
        
        // Act
        var pricing = db.GetActivePricing();
        
        // Assert
        Assert.NotEmpty(pricing);
        Assert.Equal(3, pricing.Count); // Default: 3 pricing packages
    }
}