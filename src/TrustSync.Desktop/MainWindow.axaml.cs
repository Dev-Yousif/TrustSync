using Avalonia.Controls;
using TrustSync.Desktop.ViewModels.Shell;

namespace TrustSync.Desktop;

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
}
