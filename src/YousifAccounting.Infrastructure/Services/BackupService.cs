using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly AppDbContext _db;

    public BackupService(AppDbContext db) => _db = db;

    public string GetBackupDirectory()
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YousifAccounting", "Backups");
        Directory.CreateDirectory(baseDir);
        return baseDir;
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
            var fileName = $"YousifAccounting_Backup_{timestamp}.yabak";
            var filePath = Path.Combine(backupDir, fileName);

            // Create ZIP backup
            var tempZip = Path.GetTempFileName();
            try
            {
                using (var archive = ZipFile.Open(tempZip, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(dbPath, "yousifaccounting.db");
                }
                File.Copy(tempZip, filePath, true);
            }
            finally
            {
                File.Delete(tempZip);
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
            var tempDir = Path.Combine(Path.GetTempPath(), "YousifAccounting_Restore");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

            ZipFile.ExtractToDirectory(filePath, tempDir);

            var restoredDb = Path.Combine(tempDir, "yousifaccounting.db");
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
