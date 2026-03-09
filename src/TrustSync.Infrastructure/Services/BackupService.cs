using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TrustSync.Application.Services;
using TrustSync.Domain.Common;
using TrustSync.Domain.Entities;
using TrustSync.Domain.Enums;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly AppDbContext _db;

    public BackupService(AppDbContext db) => _db = db;

    public string GetBackupDirectory()
    {
        var custom = _db.AppSettings
            .FirstOrDefault(s => s.Key == "BackupDirectory");
        if (custom is not null && !string.IsNullOrWhiteSpace(custom.Value) && Directory.Exists(custom.Value))
            return custom.Value;

        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TrustSync", "Backups");
        Directory.CreateDirectory(baseDir);
        return baseDir;
    }

    public async Task SetBackupDirectoryAsync(string path)
    {
        Directory.CreateDirectory(path);
        var setting = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "BackupDirectory");
        if (setting is not null)
            setting.Value = path;
        else
            _db.AppSettings.Add(new AppSetting { Key = "BackupDirectory", Value = path });
        await _db.SaveChangesAsync();
    }

    public async Task<Result<string>> CreateBackupAsync(string? notes = null)
    {
        try
        {
            var dbPath = DatabaseConfiguration.GetDatabasePath();
            if (!File.Exists(dbPath))
                return Result<string>.Failure("Database file not found.");

            // Checkpoint WAL
            await _db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");

            var backupDir = GetBackupDirectory();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"TrustSync_Backup_{timestamp}.yabak";
            var filePath = Path.Combine(backupDir, fileName);

            // Copy DB to temp first (the live file is locked by EF Core)
            var tempDb = Path.Combine(Path.GetTempPath(), $"yabak_{Guid.NewGuid():N}.db");
            var tempZip = Path.Combine(Path.GetTempPath(), $"yabak_{Guid.NewGuid():N}.zip");
            try
            {
                File.Copy(dbPath, tempDb, true);
                using (var archive = ZipFile.Open(tempZip, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(tempDb, "trustsync.db");
                }
                File.Copy(tempZip, filePath, true);
            }
            finally
            {
                if (File.Exists(tempDb)) File.Delete(tempDb);
                if (File.Exists(tempZip)) File.Delete(tempZip);
            }

            var fileInfo = new FileInfo(filePath);

            // Compute checksum
            using var stream = File.OpenRead(filePath);
            var hash = await SHA256.HashDataAsync(stream);
            var checksum = Convert.ToHexString(hash);

            // Record in database
            var record = new BackupRecord
            {
                FileName = fileName,
                FilePath = filePath,
                FileSizeBytes = fileInfo.Length,
                Type = BackupType.Manual,
                Checksum = checksum,
                IsEncrypted = false,
                Notes = notes
            };
            _db.BackupRecords.Add(record);
            await _db.SaveChangesAsync();

            return Result<string>.Success(filePath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Backup failed: {ex.Message}");
        }
    }

    public async Task<Result> RestoreFromBackupAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return Result.Failure("Backup file not found.");

            var dbPath = DatabaseConfiguration.GetDatabasePath();

            // Extract ZIP to temp
            var tempDir = Path.Combine(Path.GetTempPath(), "TrustSync_Restore");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

            ZipFile.ExtractToDirectory(filePath, tempDir);

            var restoredDb = Path.Combine(tempDir, "trustsync.db");
            if (!File.Exists(restoredDb)) // Fallback: support restoring legacy backups
                restoredDb = Path.Combine(tempDir, "yousifaccounting.db");
            if (!File.Exists(restoredDb))
                return Result.Failure("Backup does not contain a valid database.");

            // Close current connection
            await _db.Database.CloseConnectionAsync();

            // Safety backup of current DB
            var safetyBackup = dbPath + ".before_restore";
            if (File.Exists(dbPath))
                File.Copy(dbPath, safetyBackup, true);

            // Replace
            File.Copy(restoredDb, dbPath, true);

            // Cleanup
            Directory.Delete(tempDir, true);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Restore failed: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<BackupRecord>> GetBackupHistoryAsync()
    {
        return await _db.BackupRecords
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Result> DeleteBackupAsync(int id)
    {
        var record = await _db.BackupRecords.FindAsync(id);
        if (record is null) return Result.Failure("Backup record not found.");

        if (File.Exists(record.FilePath))
            File.Delete(record.FilePath);

        _db.BackupRecords.Remove(record);
        await _db.SaveChangesAsync();
        return Result.Success();
    }
}
