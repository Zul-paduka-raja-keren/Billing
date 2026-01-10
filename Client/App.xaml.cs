using System.Windows;
using Shared.Utils;

namespace Client;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Logger.Info("Client application started");
        
        // Handle unhandled exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            Logger.Error($"Unhandled exception: {args.Exception.Message}");
            args.Handled = true;
        };
    }
}