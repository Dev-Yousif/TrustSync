using TrustSync.Application.DTOs;

namespace TrustSync.Application.Services;

public interface IReportingService
{
    Task<IReadOnlyList<MonthlySummaryReport>> GetMonthlySummariesAsync(int year);
    Task<IReadOnlyList<IncomeBySourceItem>> GetIncomeBySourceAsync(int year, int month);
    Task<IReadOnlyList<CategoryBreakdownItem>> GetExpenseByCategoryAsync(int year, int month);
    Task<IReadOnlyList<ProjectProfitabilityItem>> GetProjectProfitabilityAsync();
    Task<string> ExportMonthlySummaryToCsvAsync(int year);
    Task<byte[]> ExportMonthlySummaryToPdfAsync(int year, string currencySymbol);
    Task<string> GetDefaultCurrencyCodeAsync();
}
