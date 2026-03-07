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
