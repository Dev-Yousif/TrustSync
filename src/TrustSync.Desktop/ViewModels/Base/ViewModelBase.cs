using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TrustSync.Desktop.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isConfirmDialogOpen;

    [ObservableProperty]
    private string _confirmDialogTitle = string.Empty;

    [ObservableProperty]
    private string _confirmDialogMessage = string.Empty;

    // Toast notification
    [ObservableProperty]
    private bool _isToastVisible;

    [ObservableProperty]
    private string _toastMessage = string.Empty;

    [ObservableProperty]
    private bool _isToastError;

    private CancellationTokenSource? _toastCts;

    private Func<Task>? _confirmAction;

    // Validation errors dictionary
    private readonly Dictionary<string, string?> _validationErrors = new();

    protected void ClearError() => ErrorMessage = null;

    protected void SetFieldError(string fieldName, string? error)
    {
        _validationErrors[fieldName] = error;
        OnPropertyChanged($"{fieldName}Error");
        OnPropertyChanged($"Has{fieldName}Error");
    }

    protected string? GetFieldError(string fieldName)
        => _validationErrors.TryGetValue(fieldName, out var error) ? error : null;

    protected bool HasFieldError(string fieldName)
        => _validationErrors.TryGetValue(fieldName, out var error) && !string.IsNullOrEmpty(error);

    protected bool HasAnyValidationError()
    {
        foreach (var kv in _validationErrors)
            if (!string.IsNullOrEmpty(kv.Value)) return true;
        return false;
    }

    protected void ClearAllFieldErrors()
    {
        var keys = new List<string>(_validationErrors.Keys);
        foreach (var key in keys)
        {
            _validationErrors[key] = null;
            OnPropertyChanged($"{key}Error");
            OnPropertyChanged($"Has{key}Error");
        }
    }

    protected async void ShowToast(string message, bool isError = false)
    {
        _toastCts?.Cancel();
        _toastCts = new CancellationTokenSource();
        var token = _toastCts.Token;

        ToastMessage = message;
        IsToastError = isError;
        IsToastVisible = true;

        try
        {
            await Task.Delay(3000, token);
            IsToastVisible = false;
        }
        catch (TaskCanceledException) { }
    }

    protected void ShowConfirmDialog(string title, string message, Func<Task> onConfirm)
    {
        ConfirmDialogTitle = title;
        ConfirmDialogMessage = message;
        _confirmAction = onConfirm;
        IsConfirmDialogOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmDialogYesAsync()
    {
        IsConfirmDialogOpen = false;
        if (_confirmAction is not null)
            await _confirmAction();
        _confirmAction = null;
    }

    [RelayCommand]
    private void ConfirmDialogNo()
    {
        IsConfirmDialogOpen = false;
        _confirmAction = null;
    }
}
