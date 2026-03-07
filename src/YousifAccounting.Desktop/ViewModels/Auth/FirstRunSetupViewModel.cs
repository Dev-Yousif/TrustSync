using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.Security;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Auth;

public partial class FirstRunSetupViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly PasswordValidator _passwordValidator;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _selectedCurrency = "USD";

    [ObservableProperty]
    private IReadOnlyList<string> _validationErrors = [];

    public string[] AvailableCurrencies { get; } =
        ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public event EventHandler? SetupCompleted;

    public FirstRunSetupViewModel(IAuthenticationService authService, PasswordValidator passwordValidator)
    {
        _authService = authService;
        _passwordValidator = passwordValidator;
    }

    partial void OnPasswordChanged(string value) => ValidatePasswordRealTime();
    partial void OnConfirmPasswordChanged(string value) => ValidatePasswordRealTime();

    private void ValidatePasswordRealTime()
    {
        if (string.IsNullOrEmpty(Password))
        {
            ValidationErrors = [];
            return;
        }

        var result = _passwordValidator.Validate(Password);
        var errors = new List<string>(result.Errors);

        if (!string.IsNullOrEmpty(ConfirmPassword) && Password != ConfirmPassword)
            errors.Add("Passwords do not match.");

        ValidationErrors = errors;
    }

    [RelayCommand]
    private async Task SetupAsync()
    {
        if (IsBusy) return;
        ClearError();

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = "Please enter your name.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        var validation = _passwordValidator.Validate(Password);
        if (!validation.IsValid)
        {
            ErrorMessage = string.Join(" ", validation.Errors);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _authService.SetupMasterPasswordAsync(DisplayName, Password, SelectedCurrency);
            if (result.IsSuccess)
            {
                SetupCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
