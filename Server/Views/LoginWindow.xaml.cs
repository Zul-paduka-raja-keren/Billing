namespace Server.Views;

using System.Windows;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        UsernameInput.Focus();
    }
    
    private void OnLoginClick(object sender, RoutedEventArgs e)
    {
        string username = UsernameInput.Text.Trim();
        string password = PasswordInput.Password;
        
        // Simple validation (replace with proper authentication)
        if (username == "admin" && password == "admin")
        {
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show(
                "Username atau password salah!",
                "Login Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            PasswordInput.Clear();
            UsernameInput.Focus();
        }
    }
}