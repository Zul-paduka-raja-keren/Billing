namespace Server.Tests;

using Xunit;
using Server.Services;
using Shared.Models;

public class BillingEngineTests
{
    [Fact]
    public void StartSession_ShouldCreateActiveSession()
    {
        // Arrange
        var mockServer = new MockTcpServerService();
        var mockDb = new MockDatabaseManager();
        var engine = new BillingEngine(mockServer, mockDb);
        
        // Act
        engine.StartSession("client1", "PC-01", 60, 5000);
        
        // Assert
        Assert.True(engine.ActiveSessions.ContainsKey("client1"));
        Assert.Equal(3600, engine.ActiveSessions["client1"].RemainingSeconds);
    }
    
    [Fact]
    public void PauseSession_ShouldSetIsPausedTrue()
    {
        // Arrange
        var mockServer = new MockTcpServerService();
        var mockDb = new MockDatabaseManager();
        var engine = new BillingEngine(mockServer, mockDb);
        engine.StartSession("client1", "PC-01", 60, 5000);
        
        // Act
        engine.PauseSession("client1");
        
        // Assert
        Assert.True(engine.ActiveSessions["client1"].IsPaused);
    }
    
    [Fact]
    public void ExtendTime_ShouldAddMinutes()
    {
        // Arrange
        var mockServer = new MockTcpServerService();
        var mockDb = new MockDatabaseManager();
        var engine = new BillingEngine(mockServer, mockDb);
        engine.StartSession("client1", "PC-01", 60, 5000);
        
        var initialTime = engine.ActiveSessions["client1"].RemainingSeconds;
        
        // Act
        engine.ExtendTime("client1", 30);
        
        // Assert
        Assert.Equal(initialTime + 1800, engine.ActiveSessions["client1"].RemainingSeconds);
    }
}