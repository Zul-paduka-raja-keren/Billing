namespace Shared.Models;

public class Pricing
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal RatePerHour { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsPackage { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}