namespace Client.Views;

using System;
using System.Windows;
using Client.Services;
using Shared.Protocol;
using Shared.Utils;
using Newtonsoft.Json.Linq;

public partial class TimerDisplay : Window
{
    private TcpClientService _tcpClient;
    private int _remainingSeconds;
    private decimal _currentCost;
    
    public TimerDisplay(TcpClientService tcpClient)
    {
        InitializeComponent();
        _tcpClient = tcpClient;
        _tcpClient.OnMessageReceived += HandleMessage;
        
        // Make draggable
        this.MouseLeftButtonDown += (s, e) => this.DragMove();
    }
    
    private void HandleMessage(Message message)
    {
        Dispatcher.Invoke(() =>
        {
            if (message.Type == MessageType.TIME_UPDATE)
            {
                var data = JObject.FromObject(message.Data ?? new { });
                _remainingSeconds = data["remaining"]?.Value<int>() ?? 0;
                
                UpdateDisplay();
                
                // Warning if less than 5 minutes
                if (_remainingSeconds <= 300 && _remainingSeconds > 295)
                {
                    MessageBox.Show("Waktu tinggal 5 menit!", "Peringatan", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if (message.Type == MessageType.LOCK)
            {
                // Show lock screen again
                var lockScreen = new LockScreen();
                lockScreen.Show();
                this.Close();
            }
        });
    }
    
    private void UpdateDisplay()
    {
        TimeText.Text = DateTimeHelper.FormatDuration(_remainingSeconds);
        
        // Update cost (example calculation)
        _currentCost = (_remainingSeconds / 3600.0m) * 5000;
        CostText.Text = $"Biaya: Rp {_currentCost:N0}";
        
        // Change color based on remaining time
        if (_remainingSeconds <= 60)
            TimeText.Foreground = System.Windows.Media.Brushes.Red;
        else if (_remainingSeconds <= 300)
            TimeText.Foreground = System.Windows.Media.Brushes.Orange;
        else
            TimeText.Foreground = System.Windows.Media.Brushes.Lime;
    }
}