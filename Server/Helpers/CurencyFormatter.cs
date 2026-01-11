namespace Server.Helpers;

using System.Globalization;

public static class CurrencyFormatter
{
    private static readonly CultureInfo IdCulture = new CultureInfo("id-ID");
    
    public static string FormatRupiah(decimal amount)
    {
        return $"Rp {amount:N0}";
    }
    
    public static string FormatRupiahDetailed(decimal amount)
    {
        return amount.ToString("C0", IdCulture);
    }
}