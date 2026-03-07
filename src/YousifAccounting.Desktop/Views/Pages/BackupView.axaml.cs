using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using YousifAccounting.Desktop.ViewModels.Pages;

namespace YousifAccounting.Desktop.Views.Pages;

public partial class BackupView : UserControl
{
    public BackupView()
    {
        InitializeComponent();
    }

    private async void OnChangeBackupDirectory(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Backup Directory",
            AllowMultiple = false
        });

        if (folders.Count > 0 && DataContext is BackupViewModel vm)
        {
            var path = folders[0].TryGetLocalPath();
            if (path is not null)
                await vm.SetBackupDirectoryFromPickerAsync(path);
        }
    }

    private async void OnImportBackup(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Backup File to Restore",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("TrustSync Backup") { Patterns = new[] { "*.yabak" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0 && DataContext is BackupViewModel vm)
        {
            var path = files[0].TryGetLocalPath();
            if (path is not null)
                await vm.ImportBackupFromFileAsync(path);
        }
    }
}
