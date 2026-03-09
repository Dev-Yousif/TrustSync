using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.Security;
using TrustSync.Desktop.ViewModels.Base;

namespace TrustSync.Desktop.ViewModels.Auth;

public partial class LockScreenViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private string _password = string.Empty;

    public event EventHandler? UnlockSucceeded;

    public LockScreenViewModel(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        if (IsBusy) return;
        ClearError();

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your password.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _authService.LoginAsync(Password);
            if (result.IsSuccess)
            {
                UnlockSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Error;
                Password = string.Empty;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
