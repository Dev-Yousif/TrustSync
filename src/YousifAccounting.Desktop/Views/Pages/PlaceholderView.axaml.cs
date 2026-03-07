using Avalonia.Controls;

namespace YousifAccounting.Desktop.Views.Pages;

public partial class PlaceholderView : UserControl
{
    public PlaceholderView()
    {
        InitializeComponent();
    }

    public PlaceholderView(string title) : this()
    {
        TitleText.Text = title;
    }
}
