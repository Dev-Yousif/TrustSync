using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.Services;

public class NavigationService
{
    private readonly IServiceProvider _services;

    public event EventHandler<ViewModelBase>? NavigationRequested;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = (TViewModel)_services.GetService(typeof(TViewModel))!;
        NavigationRequested?.Invoke(this, vm);
    }

    public void NavigateTo(Type viewModelType)
    {
        var vm = (ViewModelBase)_services.GetService(viewModelType)!;
        NavigationRequested?.Invoke(this, vm);
    }
}
