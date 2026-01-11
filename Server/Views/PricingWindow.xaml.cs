namespace Server.Views;

using System.Windows;
using Server.Services;
using Shared.Utils;

public partial class PricingWindow : Window
{
    private DatabaseManager _db;
    
    public PricingWindow(DatabaseManager db)
    {
        InitializeComponent();
        _db = db;
        LoadPricing();
    }
    
    private void LoadPricing()
    {
        var pricing = _db.GetActivePricing();
        PricingGrid.ItemsSource = pricing;
    }
    
    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Feature Tambah Paket\n\nComing soon!",
            "Add Pricing",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        if (PricingGrid.SelectedItem == null)
        {
            MessageBox.Show("Pilih paket yang akan di-edit!", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        MessageBox.Show(
            "Feature Edit Paket\n\nComing soon!",
            "Edit Pricing",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (PricingGrid.SelectedItem == null)
        {
            MessageBox.Show("Pilih paket yang akan dihapus!", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            "Yakin ingin menghapus paket ini?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            MessageBox.Show("Feature Delete - Coming soon!");
        }
    }
}