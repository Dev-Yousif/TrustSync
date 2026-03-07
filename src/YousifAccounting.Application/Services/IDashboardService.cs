using YousifAccounting.Application.DTOs;

namespace YousifAccounting.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetMonthlySummaryAsync(int year, int month);
    Task<IReadOnlyList<MonthlyDataPoint>> GetMonthlyTrendsAsync(int year);
    Task<IReadOnlyList<CategoryBreakdownItem>> GetExpenseByCategoryAsync(int year, int month);
    Task<IReadOnlyList<RecentTransactionDto>> GetRecentTransactionsAsync(int count = 10);
}
