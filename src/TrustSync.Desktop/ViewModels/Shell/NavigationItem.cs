using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TrustSync.Desktop.ViewModels.Shell;

public class NavigationItem : INotifyPropertyChanged
{
    public required string Title { get; init; }
    public required string IconKey { get; init; }
    public required Type ViewModelType { get; init; }
    public string? Group { get; init; }

    public StreamGeometry? IconGeometry
    {
        get
        {
            if (Avalonia.Application.Current is not null &&
                Avalonia.Application.Current.TryFindResource(IconKey, out var resource))
                return resource as StreamGeometry;
            return null;
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class NavigationGroup
{
    public required string Title { get; init; }
    public required IReadOnlyList<NavigationItem> Items { get; init; }
}
