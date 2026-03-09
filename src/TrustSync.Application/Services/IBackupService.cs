using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;
using TrustSync.Domain.Entities;

namespace TrustSync.Application.Services;

public interface IBackupService
{
    Task<Result<string>> CreateBackupAsync(string? notes = null);
    Task<Result> RestoreFromBackupAsync(string filePath);
    Task<IReadOnlyList<BackupRecord>> GetBackupHistoryAsync();
    Task<Result> DeleteBackupAsync(int id);
    string GetBackupDirectory();
    Task SetBackupDirectoryAsync(string path);
}
