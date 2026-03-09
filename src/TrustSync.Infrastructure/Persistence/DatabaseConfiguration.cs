namespace TrustSync.Infrastructure.Persistence;

public static class DatabaseConfiguration
{
    public static string GetDatabaseDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "TrustSync", "Data");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetDatabasePath()
    {
        var newPath = Path.Combine(GetDatabaseDirectory(), "trustsync.db");

        // Migrate from legacy path if new db doesn't exist yet
        if (!File.Exists(newPath))
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var legacyPath = Path.Combine(appData, "YousifAccounting", "Data", "yousifaccounting.db");
            if (File.Exists(legacyPath))
            {
                File.Copy(legacyPath, newPath);
            }
        }

        return newPath;
    }

    public static string GetConnectionString(string? password = null)
    {
        var dbPath = GetDatabasePath();
        var connStr = $"Data Source={dbPath}";
        if (!string.IsNullOrEmpty(password))
        {
            connStr += $";Password={password}";
        }
        return connStr;
    }

    public static string GetBackupDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "TrustSync", "Backups");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetLogDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "TrustSync", "Logs");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
