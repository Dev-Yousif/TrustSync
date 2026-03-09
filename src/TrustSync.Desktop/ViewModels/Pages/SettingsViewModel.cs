using System.Diagnostics;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using TrustSync.Application.Security;
using TrustSync.Application.Services;
using TrustSync.Desktop.ViewModels.Base;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppDbContext _db;
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;
    private readonly ICurrencyConversionService _conversionService;

    [ObservableProperty] private string _userDisplayName = "";
    [ObservableProperty] private string? _profileImagePath;
    [ObservableProperty] private Bitmap? _profileBitmap;
    [ObservableProperty] private string _defaultCurrency = "USD";
    [ObservableProperty] private int _autoLockTimeout = 5;
    // Password change
    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmNewPassword = string.Empty;
    // Exchange rates
    [ObservableProperty] private string _lastRateRefresh = "Never";
    [ObservableProperty] private bool _isRefreshingRates;
    // Auto startup
    [ObservableProperty] private bool _autoStartup;
    // Clear database
    [ObservableProperty] private string _clearDbPassword = string.Empty;
    [ObservableProperty] private string _clearDbVerification = string.Empty;
    [ObservableProperty] private bool _isClearingDatabase;

    partial void OnProfileImagePathChanged(string? value)
    {
        ProfileBitmap?.Dispose();
        ProfileBitmap = null;
        if (!string.IsNullOrEmpty(value) && File.Exists(value))
        {
            try { ProfileBitmap = new Bitmap(value); }
            catch { /* ignore corrupt images */ }
        }
    }

    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];
    public int[] TimeoutOptions { get; } = [1, 2, 5, 10, 15, 30, 60];

    public SettingsViewModel(AppDbContext db, IAuthenticationService authService, ISessionService sessionService, ICurrencyConversionService conversionService)
    {
        _db = db;
        _authService = authService;
        _sessionService = sessionService;
        _conversionService = conversionService;
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var displayName = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "UserDisplayName");
        if (displayName is not null) UserDisplayName = displayName.Value;

        var profileImg = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "ProfileImagePath");
        if (profileImg is not null && File.Exists(profileImg.Value)) ProfileImagePath = profileImg.Value;

        var currency = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultCurrency");
        if (currency is not null) DefaultCurrency = currency.Value;

        var timeout = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "AutoLockTimeoutMinutes");
        if (timeout is not null && int.TryParse(timeout.Value, out var t)) AutoLockTimeout = t;

        var lastRefresh = await _conversionService.GetLastRefreshTimeAsync();
        LastRateRefresh = lastRefresh.HasValue ? lastRefresh.Value.ToLocalTime().ToString("g") : "Never";

        var currentStartup = GetAutoStartupEnabled();
        if (!currentStartup)
        {
            // Enable auto-startup by default on first run
            SetAutoStartupEnabled(true);
            currentStartup = true;
        }
        _autoStartup = currentStartup;
        OnPropertyChanged(nameof(AutoStartup));
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsBusy = true; ClearError();
        try
        {
            var displayName = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "UserDisplayName");
            if (displayName is not null)
                displayName.Value = UserDisplayName.Trim();
            else
                _db.AppSettings.Add(new Domain.Entities.AppSetting
                    { Key = "UserDisplayName", Value = UserDisplayName.Trim(), UpdatedAt = DateTime.UtcNow });

            var profileImg = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "ProfileImagePath");
            if (profileImg is not null)
                profileImg.Value = ProfileImagePath ?? "";
            else if (!string.IsNullOrEmpty(ProfileImagePath))
                _db.AppSettings.Add(new Domain.Entities.AppSetting
                    { Key = "ProfileImagePath", Value = ProfileImagePath, UpdatedAt = DateTime.UtcNow });

            var currency = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultCurrency");
            var previousCurrency = currency?.Value ?? "USD";
            if (currency is not null) currency.Value = DefaultCurrency;

            var timeout = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "AutoLockTimeoutMinutes");
            if (timeout is not null) timeout.Value = AutoLockTimeout.ToString();

            await _db.SaveChangesAsync();
            _sessionService.SetAutoLockTimeoutMinutes(AutoLockTimeout);

            // If default currency changed, reconvert all records
            if (!string.Equals(previousCurrency, DefaultCurrency, StringComparison.OrdinalIgnoreCase))
            {
                ShowToast("Currency changed. Reconverting all records...");
                var result = await _conversionService.ReconvertAllRecordsAsync();
                if (!result.IsSuccess)
                    ShowToast(result.Error ?? "Reconversion failed.", isError: true);
                else
                    ShowToast("Settings saved. All records reconverted.");
            }
            else
            {
                ShowToast("Settings saved.");
            }
        }
        catch (Exception ex) { ShowToast(ex.Message, isError: true); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RefreshRatesAsync()
    {
        if (IsRefreshingRates) return;
        IsRefreshingRates = true;
        try
        {
            var result = await _conversionService.RefreshRatesAsync();
            if (result.IsSuccess)
            {
                var lastRefresh = await _conversionService.GetLastRefreshTimeAsync();
                LastRateRefresh = lastRefresh.HasValue ? lastRefresh.Value.ToLocalTime().ToString("g") : "Never";
                ShowToast("Exchange rates updated successfully.");
            }
            else
            {
                ShowToast(result.Error ?? "Failed to refresh rates.", isError: true);
            }
        }
        catch (Exception ex) { ShowToast(ex.Message, isError: true); }
        finally { IsRefreshingRates = false; }
    }

    public Task SetProfileImageAsync(string sourcePath)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var profileDir = Path.Combine(appData, "TrustSync", "Profile");
            Directory.CreateDirectory(profileDir);
            var ext = Path.GetExtension(sourcePath);
            var destPath = Path.Combine(profileDir, $"avatar{ext}");
            File.Copy(sourcePath, destPath, overwrite: true);
            ProfileImagePath = destPath;
            ShowToast("Profile image set. Click Save Settings to keep it.");
        }
        catch (Exception ex)
        {
            ShowToast($"Failed to set image: {ex.Message}", isError: true);
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    public void RemoveProfileImage()
    {
        ProfileImagePath = null;
        ShowToast("Profile image removed. Click Save Settings to keep it.");
    }

    [RelayCommand]
    private async Task ClearDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(ClearDbPassword))
        {
            ShowToast("Please enter your password.", isError: true);
            return;
        }
        if (!string.Equals(ClearDbVerification, "DELETE", StringComparison.Ordinal))
        {
            ShowToast("Please type DELETE to confirm.", isError: true);
            return;
        }

        IsClearingDatabase = true;
        try
        {
            var authResult = await _authService.ValidatePasswordAsync(ClearDbPassword);
            if (!authResult.IsSuccess)
            {
                ShowToast("Incorrect password.", isError: true);
                return;
            }

            // Clear all data tables (keep AppSettings, UserProfiles, Currencies, ExchangeRates)
            _db.SavingEntries.RemoveRange(await _db.SavingEntries.IgnoreQueryFilters().ToListAsync());
            _db.SavingGoals.RemoveRange(await _db.SavingGoals.IgnoreQueryFilters().ToListAsync());
            _db.Deductions.RemoveRange(await _db.Deductions.IgnoreQueryFilters().ToListAsync());
            _db.Incomes.RemoveRange(await _db.Incomes.IgnoreQueryFilters().ToListAsync());
            _db.Expenses.RemoveRange(await _db.Expenses.IgnoreQueryFilters().ToListAsync());
            _db.ProjectTags.RemoveRange(await _db.ProjectTags.ToListAsync());
            _db.Projects.RemoveRange(await _db.Projects.IgnoreQueryFilters().ToListAsync());
            _db.CompanyClients.RemoveRange(await _db.CompanyClients.IgnoreQueryFilters().ToListAsync());
            _db.Tags.RemoveRange(await _db.Tags.IgnoreQueryFilters().ToListAsync());
            _db.ExpenseCategories.RemoveRange(await _db.ExpenseCategories.IgnoreQueryFilters().ToListAsync());
            _db.AuditEntries.RemoveRange(await _db.AuditEntries.ToListAsync());
            _db.BackupRecords.RemoveRange(await _db.BackupRecords.ToListAsync());

            await _db.SaveChangesAsync();

            // Re-seed default data (expense categories, system reminders)
            var seeder = new TrustSync.Infrastructure.Persistence.Seeding.DatabaseSeeder(_db);
            await seeder.ReSeedDefaultsAsync();

            ClearDbPassword = string.Empty;
            ClearDbVerification = string.Empty;
            ShowToast("Database cleared successfully.");
        }
        catch (Exception ex) { ShowToast($"Failed to clear database: {ex.Message}", isError: true); }
        finally { IsClearingDatabase = false; }
    }

    partial void OnAutoStartupChanged(bool value)
    {
        try
        {
            SetAutoStartupEnabled(value);
            ShowToast(value ? "Auto-startup enabled." : "Auto-startup disabled.");
        }
        catch (Exception ex)
        {
            ShowToast($"Failed to set auto-startup: {ex.Message}", isError: true);
        }
    }

    private static string GetAppExePath()
    {
        var exe = Process.GetCurrentProcess().MainModule?.FileName;
        return exe ?? "";
    }

    private static bool GetAutoStartupEnabled()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("TrustSync") is not null;
            }
            if (OperatingSystem.IsLinux())
            {
                var desktopFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config", "autostart", "trustsync.desktop");
                return File.Exists(desktopFile);
            }
        }
        catch { /* ignore */ }
        return false;
    }

    private static void SetAutoStartupEnabled(bool enabled)
    {
        var exePath = GetAppExePath();

        if (OperatingSystem.IsWindows())
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key is null) return;
            if (enabled)
                key.SetValue("TrustSync", $"\"{exePath}\"");
            else
                key.DeleteValue("TrustSync", false);
        }
        else if (OperatingSystem.IsLinux())
        {
            var autostartDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "autostart");
            var desktopFile = Path.Combine(autostartDir, "trustsync.desktop");

            if (enabled)
            {
                Directory.CreateDirectory(autostartDir);
                File.WriteAllText(desktopFile,
                    $"[Desktop Entry]\nType=Application\nName=TrustSync\nExec={exePath}\nX-GNOME-Autostart-enabled=true\n");
            }
            else if (File.Exists(desktopFile))
            {
                File.Delete(desktopFile);
            }
        }
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
