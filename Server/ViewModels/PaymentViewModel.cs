namespace Server.ViewModels;

public class PaymentViewModel
{
    public string OrderId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientName { get; set; } = "";
    public decimal Amount { get; set; }
    public string Method { get; set; } = "cash";
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public string DisplayAmount => $"Rp {Amount:N0}";
    public string DisplayMethod => Method.ToUpper();
    public string DisplayStatus => Status switch
    {
        "pending" => "⏳ Menunggu",
        "success" => "✅ Sukses",
        "failed" => "❌ Gagal",
        _ => Status
    };
}