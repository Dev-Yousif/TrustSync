using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Tests;

internal static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        // Seed minimum required data
        if (!context.Currencies.Any())
        {
            context.Currencies.Add(new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 });
            context.Currencies.Add(new Currency { Code = "EUR", Name = "Euro", Symbol = "\u20AC", DecimalPlaces = 2 });
            context.SaveChanges();
        }

        return context;
    }

    public static AppDbContext CreateWithCategory(out int categoryId, string? dbName = null)
    {
        var context = Create(dbName);
        var category = new ExpenseCategory { Name = "Food", ColorHex = "#FF0000", SortOrder = 1 };
        context.ExpenseCategories.Add(category);
        context.SaveChanges();
        categoryId = category.Id;
        return context;
    }
}

internal sealed class NullAuditService : IAuditService
{
    public Task LogAsync(AuditAction action, string entityType, int? entityId = null, string? details = null)
        => Task.CompletedTask;
}

internal sealed class NullCurrencyConversionService : ICurrencyConversionService
{
    public Task<YousifAccounting.Domain.Common.Result<YousifAccounting.Application.DTOs.ConversionResult>> ConvertToDefaultAsync(decimal amount, string fromCurrencyCode)
        => Task.FromResult(YousifAccounting.Domain.Common.Result<YousifAccounting.Application.DTOs.ConversionResult>.Success(
            new YousifAccounting.Application.DTOs.ConversionResult
            {
                ConvertedAmount = amount,
                ExchangeRateUsed = 1m,
                TargetCurrencyCode = fromCurrencyCode
            }));

    public Task<YousifAccounting.Domain.Common.Result> RefreshRatesAsync()
        => Task.FromResult(YousifAccounting.Domain.Common.Result.Success());

    public Task<DateTime?> GetLastRefreshTimeAsync()
        => Task.FromResult<DateTime?>(null);

    public Task<YousifAccounting.Domain.Common.Result> ReconvertAllRecordsAsync()
        => Task.FromResult(YousifAccounting.Domain.Common.Result.Success());
}
