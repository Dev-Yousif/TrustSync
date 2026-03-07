using Avalonia.Controls;
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
}
