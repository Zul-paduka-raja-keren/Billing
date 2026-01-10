namespace Shared.Models;

public class Payment
{
    public string OrderId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "cash";
    public string Status { get; set; } = "pending";
    public string? QrCodeUrl { get; set; }
    public string? DeepLink { get; set; }
    public string? MidtransTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}