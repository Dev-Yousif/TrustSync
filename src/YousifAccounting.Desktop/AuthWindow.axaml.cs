using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace YousifAccounting.Desktop;

public partial class AuthWindow : Window
{
    public AuthWindow()
    {
        InitializeComponent();
        PointerPressed += OnWindowPointerPressed;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void OnClose(object? sender, RoutedEventArgs e)
        => Close();

    public void SetContent(object view)
    {
        AuthContent.Content = view;
    }
}
