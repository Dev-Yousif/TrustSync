using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.Security;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Auth;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private int _failedAttempts;

    [ObservableProperty]
    private bool _isCoolingDown;

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy || IsCoolingDown) return;
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
                FailedAttempts = 0;
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                FailedAttempts++;
                ErrorMessage = result.Error;
                Password = string.Empty;

                if (FailedAttempts >= 5)
                {
                    IsCoolingDown = true;
                    ErrorMessage = "Too many failed attempts. Please wait 30 seconds.";
                    await Task.Delay(30_000);
                    IsCoolingDown = false;
                    FailedAttempts = 0;
                    ClearError();
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
