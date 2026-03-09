using Microsoft.EntityFrameworkCore;
using TrustSync.Domain.Entities;
using TrustSync.Domain.Enums;

namespace TrustSync.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Always ensure system reminders exist (even for existing databases)
        await SeedDefaultRemindersAsync();

        // Re-seed expense categories if they were cleared
        if (!await _context.ExpenseCategories.AnyAsync())
            await SeedExpenseCategoriesAsync();

        if (await _context.Currencies.AnyAsync())
        {
            // Save any new system reminders / categories for existing databases
            if (_context.ChangeTracker.HasChanges())
                await _context.SaveChangesAsync();
            return;
        }

        await SeedCurrenciesAsync();
        await SeedExpenseCategoriesAsync();
        await SeedDefaultSettingsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedCurrenciesAsync()
    {
        var currencies = new Currency[]
        {
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2 },
            new() { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2 },
            new() { Code = "IQD", Name = "Iraqi Dinar", Symbol = "ع.د", DecimalPlaces = 0 },
            new() { Code = "AED", Name = "UAE Dirham", Symbol = "د.إ", DecimalPlaces = 2 },
            new() { Code = "SAR", Name = "Saudi Riyal", Symbol = "﷼", DecimalPlaces = 2 },
            new() { Code = "TRY", Name = "Turkish Lira", Symbol = "₺", DecimalPlaces = 2 },
            new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "CA$", DecimalPlaces = 2 },
            new() { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2 },
            new() { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0 },
        };
        await _context.Currencies.AddRangeAsync(currencies);
    }

    private async Task SeedExpenseCategoriesAsync()
    {
        var now = DateTime.UtcNow;
        var categories = new ExpenseCategory[]
        {
            new() { Name = "Housing & Rent", Icon = "Home", ColorHex = "#5B8DEF", IsDefault = true, SortOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Food & Dining", Icon = "Restaurant", ColorHex = "#F5A623", IsDefault = true, SortOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Transportation", Icon = "Car", ColorHex = "#7ED321", IsDefault = true, SortOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Utilities", Icon = "Bolt", ColorHex = "#BD10E0", IsDefault = true, SortOrder = 4, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Internet & Phone", Icon = "Wifi", ColorHex = "#4A90D9", IsDefault = true, SortOrder = 5, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Health & Medical", Icon = "Heart", ColorHex = "#D0021B", IsDefault = true, SortOrder = 6, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Insurance", Icon = "Shield", ColorHex = "#8B572A", IsDefault = true, SortOrder = 7, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Subscriptions", Icon = "CreditCard", ColorHex = "#9013FE", IsDefault = true, SortOrder = 8, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Shopping", Icon = "ShoppingBag", ColorHex = "#E8D44D", IsDefault = true, SortOrder = 9, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Education", Icon = "Book", ColorHex = "#50E3C2", IsDefault = true, SortOrder = 10, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Entertainment", Icon = "Film", ColorHex = "#E91E63", IsDefault = true, SortOrder = 11, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Software & Tools", Icon = "Code", ColorHex = "#00BCD4", IsDefault = true, SortOrder = 12, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Hardware & Equipment", Icon = "Monitor", ColorHex = "#607D8B", IsDefault = true, SortOrder = 13, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Travel", Icon = "Plane", ColorHex = "#FF5722", IsDefault = true, SortOrder = 14, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Gifts & Donations", Icon = "Gift", ColorHex = "#E040FB", IsDefault = true, SortOrder = 15, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Taxes & Fees", Icon = "FileText", ColorHex = "#795548", IsDefault = true, SortOrder = 16, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Personal Care", Icon = "User", ColorHex = "#FF9800", IsDefault = true, SortOrder = 17, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Work Expenses", Icon = "Briefcase", ColorHex = "#3F51B5", IsDefault = true, SortOrder = 18, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Miscellaneous", Icon = "MoreHorizontal", ColorHex = "#9E9E9E", IsDefault = true, SortOrder = 99, CreatedAt = now, UpdatedAt = now },
        };
        await _context.ExpenseCategories.AddRangeAsync(categories);
    }

    private async Task SeedDefaultSettingsAsync()
    {
        var now = DateTime.UtcNow;
        var settings = new AppSetting[]
        {
            new() { Key = "Theme", Value = "Dark", Description = "Application theme (Dark/Light)", UpdatedAt = now },
            new() { Key = "DefaultCurrency", Value = "USD", Description = "Default currency for new entries", UpdatedAt = now },
            new() { Key = "AutoBackupEnabled", Value = "true", Description = "Enable automatic backup on app start", UpdatedAt = now },
            new() { Key = "BackupRetentionCount", Value = "30", Description = "Maximum number of backup files to keep", UpdatedAt = now },
            new() { Key = "AutoLockTimeoutMinutes", Value = "5", Description = "Minutes of inactivity before auto-lock (0 = disabled)", UpdatedAt = now },
            new() { Key = "BackupPath", Value = "", Description = "Custom backup directory path (empty = default)", UpdatedAt = now },
        };
        await _context.AppSettings.AddRangeAsync(settings);
    }

    /// <summary>
    /// Re-seeds expense categories and system reminders after a database clear.
    /// </summary>
    public async Task ReSeedDefaultsAsync()
    {
        if (!await _context.ExpenseCategories.AnyAsync())
            await SeedExpenseCategoriesAsync();

        if (!await _context.Reminders.AnyAsync(r => r.IsSystem))
        {
            var now = DateTime.UtcNow;
            var reminders = new Reminder[]
            {
                new()
                {
                    Title = "Log your expenses today",
                    Description = "Don't forget to add your expenses before the day ends! Keep your records up to date.",
                    IsEnabled = true, IsSystem = true,
                    RepeatType = RepeatType.Daily, TimeOfDay = new TimeOnly(20, 0),
                    NextFireAt = DateTime.Today.AddHours(20).ToUniversalTime(),
                    CreatedAt = now, UpdatedAt = now
                },
                new()
                {
                    Title = "Last chance — add your expenses!",
                    Description = "The day is almost over. Make sure you haven't forgotten any expenses before you sleep.",
                    IsEnabled = true, IsSystem = true,
                    RepeatType = RepeatType.Daily, TimeOfDay = new TimeOnly(22, 0),
                    NextFireAt = DateTime.Today.AddHours(22).ToUniversalTime(),
                    CreatedAt = now, UpdatedAt = now
                },
            };
            await _context.Reminders.AddRangeAsync(reminders);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDefaultRemindersAsync()
    {
        if (await _context.Reminders.AnyAsync(r => r.IsSystem))
            return;

        // Remove old non-system seeded reminders if upgrading
        var oldSeeded = await _context.Reminders
            .Where(r => !r.IsSystem && (r.Title == "Add your daily expenses" || r.Title == "Review monthly income"
                || r.Title == "End of month financial review" || r.Title == "Backup your data"))
            .ToListAsync();
        if (oldSeeded.Count > 0)
            _context.Reminders.RemoveRange(oldSeeded);

        var now = DateTime.UtcNow;
        var reminders = new Reminder[]
        {
            new()
            {
                Title = "Log your expenses today",
                Description = "Don't forget to add your expenses before the day ends! Keep your records up to date.",
                IsEnabled = true,
                IsSystem = true,
                RepeatType = RepeatType.Daily,
                TimeOfDay = new TimeOnly(20, 0),
                NextFireAt = DateTime.Today.AddHours(20).ToUniversalTime(),
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Title = "Last chance — add your expenses!",
                Description = "The day is almost over. Make sure you haven't forgotten any expenses before you sleep.",
                IsEnabled = true,
                IsSystem = true,
                RepeatType = RepeatType.Daily,
                TimeOfDay = new TimeOnly(22, 0),
                NextFireAt = DateTime.Today.AddHours(22).ToUniversalTime(),
                CreatedAt = now, UpdatedAt = now
            },
        };
        await _context.Reminders.AddRangeAsync(reminders);
    }

}
