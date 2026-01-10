namespace Shared.Config;

public class AppConfig
{
    public ServerConfig Server { get; set; } = new();
    public DatabaseConfig Database { get; set; } = new();
    public PaymentConfig Payment { get; set; } = new();
    public PricingConfig Pricing { get; set; } = new();
    public FeaturesConfig Features { get; set; } = new();
}

public class ServerConfig
{
    public int Port { get; set; } = 9999;
    public int MaxClients { get; set; } = 50;
    public bool EnableAutoBackup { get; set; } = true;
    public int BackupIntervalHours { get; set; } = 24;
}

public class DatabaseConfig
{
    public string Path { get; set; } = "Data/billing.db";
    public string BackupPath { get; set; } = "Data/Backups/";
}

public class PaymentConfig
{
    public string MidtransServerKey { get; set; } = "";
    public string MidtransClientKey { get; set; } = "";
    public bool IsProduction { get; set; } = false;
    public string WebhookUrl { get; set; } = "";
}

public class PricingConfig
{
    public decimal DefaultRatePerHour { get; set; } = 5000;
    public int GracePeriodSeconds { get; set; } = 30;
    public int WarningTimeSeconds { get; set; } = 300;
}

public class FeaturesConfig
{
    public bool EnableIdleDetection { get; set; } = true;
    public int IdleTimeoutMinutes { get; set; } = 10;
    public bool EnableProcessMonitor { get; set; } = true;
    public List<string> BlacklistedProcesses { get; set; } = new() { "cheatengine", "processhacker" };
}