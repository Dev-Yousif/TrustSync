using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;

namespace TrustSync.Desktop.Views.Auth;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnDeveloperLinkClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is TextBlock tb && tb.Tag is string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
