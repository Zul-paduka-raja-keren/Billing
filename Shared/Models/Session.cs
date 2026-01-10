namespace Shared.Models;

public class Session
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal RatePerHour { get; set; }
    public decimal TotalCost { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public string Status { get; set; } = "active";
    public string? Notes { get; set; }
}