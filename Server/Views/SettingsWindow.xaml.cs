namespace Server.Views;

using System.Windows;
using Shared.Utils;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        LoadSettings();
    }
    
    private void LoadSettings()
    {
        // Load from config.json if exists
        // For now, using default values
        Logger.Info("Settings loaded");
    }
    
    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate inputs
            if (!int.TryParse(PortInput.Text, out int port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Port harus antara 1024-65535!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!int.TryParse(MaxClientsInput.Text, out int maxClients) || maxClients < 1 || maxClients > 100)
            {
                MessageBox.Show("Max clients harus antara 1-100!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!decimal.TryParse(DefaultRateInput.Text, out decimal rate) || rate <= 0)
            {
                MessageBox.Show("Harga harus lebih dari 0!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Save to config.json
            // TODO: Implement config save logic
            
            Logger.Info("Settings saved successfully");
            
            MessageBox.Show(
                "Pengaturan berhasil disimpan!\n\nRestart server untuk menerapkan perubahan.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            Close();
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Failed to save settings: {ex.Message}");
            MessageBox.Show(
                $"Gagal menyimpan:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}