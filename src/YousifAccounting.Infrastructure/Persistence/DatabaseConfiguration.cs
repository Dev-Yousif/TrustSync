namespace YousifAccounting.Infrastructure.Persistence;

public static class DatabaseConfiguration
{
    public static string GetDatabaseDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "YousifAccounting", "Data");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetDatabasePath()
    {
        return Path.Combine(GetDatabaseDirectory(), "yousifaccounting.db");
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
        var folder = Path.Combine(appData, "YousifAccounting", "Backups");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetLogDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "YousifAccounting", "Logs");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
