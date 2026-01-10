namespace Server;

using System.Windows;
using Shared.Utils;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Logger.Info("Server application started");
        
        DispatcherUnhandledException += (s, args) =>
        {
            Logger.Error($"Unhandled exception: {args.Exception.Message}");
            MessageBox.Show($"Error: {args.Exception.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}