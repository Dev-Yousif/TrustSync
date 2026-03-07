using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.Security;
using YousifAccounting.Desktop.ViewModels.Base;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppDbContext _db;
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;

    [ObservableProperty] private string _defaultCurrency = "USD";
    [ObservableProperty] private int _autoLockTimeout = 5;
    // Password change
    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmNewPassword = string.Empty;

    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];
    public int[] TimeoutOptions { get; } = [1, 2, 5, 10, 15, 30, 60];

    public SettingsViewModel(AppDbContext db, IAuthenticationService authService, ISessionService sessionService)
    {
        _db = db;
        _authService = authService;
        _sessionService = sessionService;
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var currency = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultCurrency");
        if (currency is not null) DefaultCurrency = currency.Value;

        var timeout = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "AutoLockTimeoutMinutes");
        if (timeout is not null && int.TryParse(timeout.Value, out var t)) AutoLockTimeout = t;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsBusy = true; ClearError();
        try
        {
            var currency = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultCurrency");
            if (currency is not null) currency.Value = DefaultCurrency;

            var timeout = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "AutoLockTimeoutMinutes");
            if (timeout is not null) timeout.Value = AutoLockTimeout.ToString();

            await _db.SaveChangesAsync();
            _sessionService.SetAutoLockTimeoutMinutes(AutoLockTimeout);
            ShowToast("Settings saved.");
        }
        catch (Exception ex) { ShowToast(ex.Message, isError: true); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword)) { ShowToast("Current password is required.", isError: true); return; }
        if (NewPassword != ConfirmNewPassword) { ShowToast("New passwords do not match.", isError: true); return; }

        IsBusy = true; ClearError();
        try
        {
            var result = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);
            if (result.IsSuccess)
            {
                ShowToast("Password changed successfully.");
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
            }
            else
                ShowToast(result.Error ?? "Password change failed.", isError: true);
        }
        finally { IsBusy = false; }
    }
}
