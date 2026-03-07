using Avalonia.Controls;
using Avalonia.Interactivity;
using YousifAccounting.Desktop.ViewModels.Shell;

namespace YousifAccounting.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnMinimize(object? sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void OnClose(object? sender, RoutedEventArgs e)
        => Close();
}
