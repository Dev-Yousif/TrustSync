using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TrustSync.Desktop.ViewModels.Base;

public abstract partial class CrudViewModelBase<TDto> : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<TDto> _items = [];

    [ObservableProperty]
    private TDto? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isEditorOpen;

    public abstract Task LoadItemsAsync();

    partial void OnSearchTextChanged(string value)
    {
        FilterItems();
    }

    protected virtual void FilterItems() { }
}
