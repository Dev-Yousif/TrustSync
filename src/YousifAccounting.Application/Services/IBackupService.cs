using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;

namespace YousifAccounting.Application.Services;

public interface IBackupService
{
    Task<Result<string>> CreateBackupAsync(string? notes = null);
    Task<Result> RestoreFromBackupAsync(string filePath);
    Task<IReadOnlyList<BackupRecord>> GetBackupHistoryAsync();
    Task<Result> DeleteBackupAsync(int id);
    string GetBackupDirectory();
    Task SetBackupDirectoryAsync(string path);
}
