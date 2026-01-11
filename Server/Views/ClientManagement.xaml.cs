namespace Server.Views;

using System;
using System.Windows;
using Server.Services;
using Shared.Utils;

public partial class ClientManagement : Window
{
    private DatabaseManager _db;
    
    public ClientManagement(DatabaseManager db)
    {
        InitializeComponent();
        _db = db;
        
        ClientNameInput.Focus();
    }
    
    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        string name = ClientNameInput.Text.Trim();
        string ip = IpAddressInput.Text.Trim();
        string mac = MacAddressInput.Text.Trim();
        
        // Validation
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show(
                "Nama client tidak boleh kosong!",
                "Validasi Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            ClientNameInput.Focus();
            return;
        }
        
        // Generate unique ID
        string clientId = GenerateClientId(name);
        
        // Save to database
        try
        {
            _db.UpsertClient(new Shared.Models.Client
            {
                Id = clientId,
                Name = name,
                IpAddress = string.IsNullOrEmpty(ip) ? null : ip,
                MacAddress = string.IsNullOrEmpty(mac) ? null : mac,
                IsOnline = false,
                CreatedAt = DateTime.Now
            });
            
            Logger.Info($"New client added: {name} ({clientId})");
            
            MessageBox.Show(
                $"Client berhasil ditambahkan!\n\n" +
                $"ID: {clientId}\n" +
                $"Nama: {name}\n" +
                $"IP: {(string.IsNullOrEmpty(ip) ? "Auto" : ip)}",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save client: {ex.Message}");
            MessageBox.Show(
                $"Gagal menyimpan client:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    
    private string GenerateClientId(string name)
    {
        // Generate ID: PC-{name-prefix}-{random}
        string prefix = name.Length >= 3 
            ? name.Substring(0, 3).ToUpper() 
            : name.ToUpper();
        
        string random = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        
        return $"PC-{prefix}-{random}";
    }
    
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}