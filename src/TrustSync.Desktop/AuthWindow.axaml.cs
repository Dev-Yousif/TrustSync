using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace TrustSync.Desktop;

public partial class AuthWindow : Window
{
    public AuthWindow()
    {
        InitializeComponent();
        PointerPressed += OnWindowPointerPressed;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            && e.Source is not Button
            && !(e.Source is Avalonia.Visual v && v.FindAncestorOfType<Button>() is not null))
            BeginMoveDrag(e);
    }

    private void OnClose(object? sender, RoutedEventArgs e)
        => Close();

    public void SetContent(object view)
    {
        AuthContent.Content = view;
    }
}
