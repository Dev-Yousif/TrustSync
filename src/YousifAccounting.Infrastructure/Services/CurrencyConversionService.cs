using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";

    public CurrencyConversionService(AppDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<ConversionResult>> ConvertToDefaultAsync(decimal amount, string fromCurrencyCode)
    {
        var defaultCurrency = await GetDefaultCurrencyAsync();

        if (string.Equals(fromCurrencyCode, defaultCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ConversionResult>.Success(new ConversionResult
            {
                ConvertedAmount = amount,
                ExchangeRateUsed = 1m,
                TargetCurrencyCode = defaultCurrency
            });
        }

        var rates = await GetCachedRatesAsync();
        if (rates.Count == 0)
        {
            return Result<ConversionResult>.Failure("No exchange rates available.");
        }

        var fromUpper = fromCurrencyCode.ToUpperInvariant();
        var defaultUpper = defaultCurrency.ToUpperInvariant();

        if (!rates.TryGetValue(fromUpper, out var fromRate) || fromRate == 0)
        {
            return Result<ConversionResult>.Failure($"No rate found for {fromCurrencyCode}.");
        }

        decimal convertedAmount;
        decimal effectiveRate;

        if (defaultUpper == "USD")
        {
            // from → USD: divide by fromRate
            convertedAmount = amount / fromRate;
            effectiveRate = fromRate;
        }
        else if (fromUpper == "USD")
        {
            // USD → target: multiply by targetRate
            if (!rates.TryGetValue(defaultUpper, out var targetRate) || targetRate == 0)
                return Result<ConversionResult>.Failure($"No rate found for {defaultCurrency}.");
            convertedAmount = amount * targetRate;
            effectiveRate = targetRate;
        }
        else
        {
            // Cross-rate: from → USD → target
            if (!rates.TryGetValue(defaultUpper, out var targetRate) || targetRate == 0)
                return Result<ConversionResult>.Failure($"No rate found for {defaultCurrency}.");
            convertedAmount = amount / fromRate * targetRate;
            effectiveRate = fromRate / targetRate;
        }

        convertedAmount = Math.Round(convertedAmount, 2);

        return Result<ConversionResult>.Success(new ConversionResult
        {
            ConvertedAmount = convertedAmount,
            ExchangeRateUsed = effectiveRate,
            TargetCurrencyCode = defaultCurrency
        });
    }

    public async Task<Result> RefreshRatesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(ApiUrl);
            var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("result", out var resultProp) ||
                resultProp.GetString() != "success")
            {
                return Result.Failure("API returned an error.");
            }

            if (!root.TryGetProperty("rates", out var ratesElement))
            {
                return Result.Failure("No rates in API response.");
            }

            var now = DateTime.UtcNow;
            foreach (var rate in ratesElement.EnumerateObject())
            {
                var code = rate.Name.ToUpperInvariant();
                if (code.Length > 3) continue;

                var rateValue = rate.Value.GetDecimal();
                var existing = await _db.ExchangeRates.FindAsync(code);
                if (existing != null)
                {
                    existing.RateToUsd = rateValue;
                    existing.FetchedAt = now;
                }
                else
                {
                    _db.ExchangeRates.Add(new ExchangeRate
                    {
                        CurrencyCode = code,
                        RateToUsd = rateValue,
                        FetchedAt = now
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to refresh rates: {ex.Message}");
        }
    }

    public async Task<DateTime?> GetLastRefreshTimeAsync()
    {
        var latest = await _db.ExchangeRates
            .OrderByDescending(r => r.FetchedAt)
            .FirstOrDefaultAsync();
        return latest?.FetchedAt;
    }

    public async Task<Result> ReconvertAllRecordsAsync()
    {
        try
        {
            var rates = await GetCachedRatesAsync();
            if (rates.Count == 0)
            {
                var refreshResult = await RefreshRatesAsync();
                if (!refreshResult.IsSuccess) return refreshResult;
                rates = await GetCachedRatesAsync();
            }

            var defaultCurrency = await GetDefaultCurrencyAsync();

            // Reconvert Incomes
            var incomes = await _db.Incomes.IgnoreQueryFilters().ToListAsync();
            foreach (var income in incomes)
            {
                var result = await ConvertAmount(income.Amount, income.CurrencyCode, defaultCurrency, rates);
                income.ConvertedAmount = result.amount;
                income.ConvertedCurrencyCode = result.currency;
                income.ExchangeRateUsed = result.rate;
            }

            // Reconvert Expenses
            var expenses = await _db.Expenses.IgnoreQueryFilters().ToListAsync();
            foreach (var expense in expenses)
            {
                var result = await ConvertAmount(expense.Amount, expense.CurrencyCode, defaultCurrency, rates);
                expense.ConvertedAmount = result.amount;
                expense.ConvertedCurrencyCode = result.currency;
                expense.ExchangeRateUsed = result.rate;
            }

            // Reconvert Deductions
            var deductions = await _db.Deductions.IgnoreQueryFilters().ToListAsync();
            foreach (var deduction in deductions)
            {
                var result = await ConvertAmount(deduction.Amount, deduction.CurrencyCode, defaultCurrency, rates);
                deduction.ConvertedAmount = result.amount;
                deduction.ConvertedCurrencyCode = result.currency;
                deduction.ExchangeRateUsed = result.rate;
            }

            // Reconvert SavingGoals
            var goals = await _db.SavingGoals.IgnoreQueryFilters().ToListAsync();
            foreach (var goal in goals)
            {
                var result = await ConvertAmount(goal.TargetAmount, goal.CurrencyCode, defaultCurrency, rates);
                goal.ConvertedTargetAmount = result.amount;
                goal.ConvertedCurrencyCode = result.currency;
                goal.ExchangeRateUsed = result.rate;
            }

            // Reconvert SavingEntries
            var entries = await _db.SavingEntries.IgnoreQueryFilters()
                .Include(e => e.SavingGoal)
                .ToListAsync();
            foreach (var entry in entries)
            {
                var currencyCode = entry.SavingGoal?.CurrencyCode ?? "USD";
                var result = await ConvertAmount(entry.Amount, currencyCode, defaultCurrency, rates);
                entry.ConvertedAmount = result.amount;
                entry.ConvertedCurrencyCode = result.currency;
                entry.ExchangeRateUsed = result.rate;
            }

            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to reconvert records: {ex.Message}");
        }
    }

    private async Task<string> GetDefaultCurrencyAsync()
    {
        return await _db.AppSettings
            .Where(s => s.Key == "DefaultCurrency")
            .Select(s => s.Value)
            .FirstOrDefaultAsync() ?? "USD";
    }

    private async Task<Dictionary<string, decimal>> GetCachedRatesAsync()
    {
        var rates = await _db.ExchangeRates.ToListAsync();

        // Auto-refresh if stale (>24h) or empty
        if (rates.Count == 0 || rates.Any(r => (DateTime.UtcNow - r.FetchedAt).TotalHours > 24))
        {
            var refreshResult = await RefreshRatesAsync();
            if (refreshResult.IsSuccess)
            {
                rates = await _db.ExchangeRates.ToListAsync();
            }
        }

        return rates.ToDictionary(r => r.CurrencyCode, r => r.RateToUsd);
    }

    private Task<(decimal amount, string currency, decimal rate)> ConvertAmount(
        decimal amount, string fromCurrency, string defaultCurrency, Dictionary<string, decimal> rates)
    {
        if (string.Equals(fromCurrency, defaultCurrency, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult((amount, defaultCurrency, 1m));

        var fromUpper = fromCurrency.ToUpperInvariant();
        var defaultUpper = defaultCurrency.ToUpperInvariant();

        if (!rates.TryGetValue(fromUpper, out var fromRate) || fromRate == 0)
            return Task.FromResult((amount, fromCurrency, 1m));

        decimal converted;
        decimal effectiveRate;

        if (defaultUpper == "USD")
        {
            converted = amount / fromRate;
            effectiveRate = fromRate;
        }
        else if (fromUpper == "USD")
        {
            if (!rates.TryGetValue(defaultUpper, out var targetRate) || targetRate == 0)
                return Task.FromResult((amount, fromCurrency, 1m));
            converted = amount * targetRate;
            effectiveRate = targetRate;
        }
        else
        {
            if (!rates.TryGetValue(defaultUpper, out var targetRate) || targetRate == 0)
                return Task.FromResult((amount, fromCurrency, 1m));
            converted = amount / fromRate * targetRate;
            effectiveRate = fromRate / targetRate;
        }

        return Task.FromResult((Math.Round(converted, 2), defaultCurrency, effectiveRate));
    }
}
