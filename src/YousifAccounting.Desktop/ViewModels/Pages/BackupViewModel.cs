using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class BackupViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;

    [ObservableProperty] private ObservableCollection<BackupRecord> _backups = [];
    [ObservableProperty] private string _backupDirectory = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public BackupViewModel(IBackupService backupService)
    {
        _backupService = backupService;
        BackupDirectory = _backupService.GetBackupDirectory();
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Backups = new ObservableCollection<BackupRecord>(await _backupService.GetBackupHistoryAsync());
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        IsBusy = true; StatusMessage = string.Empty; ClearError();
        try
        {
            var result = await _backupService.CreateBackupAsync();
            if (result.IsSuccess)
            {
                StatusMessage = $"Backup created: {Path.GetFileName(result.Value)}";
                await LoadAsync();
                ShowToast("Backup created successfully.");
            }
            else
            {
                ErrorMessage = result.Error;
                ShowToast(result.Error ?? "Backup failed.", isError: true);
            }
        }
        finally { IsBusy = false; }
    }

    public async Task SetBackupDirectoryFromPickerAsync(string path)
    {
        IsBusy = true; ClearError();
        try
        {
            await _backupService.SetBackupDirectoryAsync(path);
            BackupDirectory = _backupService.GetBackupDirectory();
            ShowToast("Backup directory updated.");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to set directory: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    public Task ImportBackupFromFileAsync(string filePath)
    {
        ShowConfirmDialog(
            "Restore Backup",
            "This will replace your current data with the backup. A safety copy of your current database will be saved. Are you sure?",
            async () =>
            {
                IsBusy = true; ClearError(); StatusMessage = string.Empty;
                try
                {
                    var result = await _backupService.RestoreFromBackupAsync(filePath);
                    if (result.IsSuccess)
                    {
                        StatusMessage = "Backup restored successfully. Please restart the app.";
                        ShowToast("Backup restored. Restart the app to apply.");
                    }
                    else
                    {
                        ErrorMessage = result.Error;
                        ShowToast(result.Error ?? "Restore failed.", isError: true);
                    }
                }
                finally { IsBusy = false; }
            });
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void RequestDeleteBackup(BackupRecord record)
    {
        ShowConfirmDialog("Delete Backup", $"Are you sure you want to delete \"{record.FileName}\"?", async () =>
        {
            IsBusy = true; ClearError();
            try
            {
                var result = await _backupService.DeleteBackupAsync(record.Id);
                if (result.IsSuccess)
                {
                    await LoadAsync();
                    ShowToast("Backup deleted.");
                }
                else
                    ErrorMessage = result.Error;
            }
            finally { IsBusy = false; }
        });
    }
}
