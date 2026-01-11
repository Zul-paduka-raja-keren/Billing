namespace Server.Views;

using System.Windows;
using Shared.Utils;

public partial class PaymentDialog : Window
{
    private string _clientId;
    private string _clientName;
    
    public PaymentDialog(string clientId, string clientName)
    {
        InitializeComponent();
        
        _clientId = clientId;
        _clientName = clientName;
        
        ClientNameText.Text = $"Client: {clientName}";
    }
    
    private void QuickAmount_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as System.Windows.Controls.Button;
        if (button?.Tag != null)
        {
            AmountInput.Text = button.Tag.ToString();
        }
    }
    
    private void OnCashClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateAmount(out decimal amount)) return;
        
        Logger.Info($"Cash payment: {_clientName} - Rp {amount:N0}");
        
        MessageBox.Show(
            $"Pembayaran Cash:\nJumlah: Rp {amount:N0}\n\nSilakan terima pembayaran dari customer.",
            "Cash Payment",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        
        DialogResult = true;
        Close();
    }
    
    private void OnQRISClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateAmount(out decimal amount)) return;
        
        Logger.Info($"QRIS payment requested: {_clientName} - Rp {amount:N0}");
        
        MessageBox.Show(
            "QRIS Payment Integration\n\n" +
            "Feature ini memerlukan Midtrans API.\n" +
            "Silakan setup Midtrans terlebih dahulu di config.json",
            "QRIS Payment",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    private void OnGopayClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateAmount(out decimal amount)) return;
        
        Logger.Info($"GoPay payment requested: {_clientName} - Rp {amount:N0}");
        
        MessageBox.Show(
            "GoPay Payment Integration\n\n" +
            "Feature coming soon!\nSaat ini hanya tersedia Cash payment.",
            "GoPay Payment",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    private void OnOVOClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateAmount(out decimal amount)) return;
        
        Logger.Info($"OVO payment requested: {_clientName} - Rp {amount:N0}");
        
        MessageBox.Show(
            "OVO Payment Integration\n\n" +
            "Feature coming soon!\nSaat ini hanya tersedia Cash payment.",
            "OVO Payment",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    private bool ValidateAmount(out decimal amount)
    {
        if (!decimal.TryParse(AmountInput.Text, out amount) || amount <= 0)
        {
            MessageBox.Show(
                "Jumlah pembayaran tidak valid!",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            AmountInput.Focus();
            return false;
        }
        
        return true;
    }
    
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}