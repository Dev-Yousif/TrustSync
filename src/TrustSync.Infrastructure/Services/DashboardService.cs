using Microsoft.EntityFrameworkCore;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Domain.Enums;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetMonthlySummaryAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var monthlyIncome = (decimal)await _db.Incomes
            .Where(i => i.Date >= startDate && i.Date < endDate)
            .SumAsync(i => (double)i.ConvertedAmount);

        var monthlyExpenses = (decimal)await _db.Expenses
            .Where(e => e.Date >= startDate && e.Date < endDate)
            .SumAsync(e => (double)e.ConvertedAmount);

        var monthlyDeductions = (decimal)await _db.Deductions
            .Where(d => d.IsActive &&
                   d.StartDate <= endDate &&
                   (d.EndDate == null || d.EndDate >= startDate))
            .SumAsync(d => (double)d.ConvertedAmount);

        var monthlySavings = (decimal)await _db.SavingEntries
            .Where(s => s.Date >= startDate && s.Date < endDate)
            .SumAsync(s => (double)s.ConvertedAmount);

        var activeProjects = await _db.Projects
            .CountAsync(p => p.Status == ProjectStatus.InProgress);

        var companiesCount = await _db.CompanyClients
            .CountAsync(c => c.Status == CompanyStatus.Active);

        var defaultCurrency = await _db.AppSettings
            .Where(s => s.Key == "DefaultCurrency")
            .Select(s => s.Value)
            .FirstOrDefaultAsync() ?? "USD";

        var userName = await _db.AppSettings
            .Where(s => s.Key == "UserDisplayName")
            .Select(s => s.Value)
            .FirstOrDefaultAsync() ?? "";

        var profileImage = await _db.AppSettings
            .Where(s => s.Key == "ProfileImagePath")
            .Select(s => s.Value)
            .FirstOrDefaultAsync();

        // Previous month data for insights
        var prevStart = startDate.AddMonths(-1);
        var prevEnd = startDate;

        var prevIncome = (decimal)await _db.Incomes
            .Where(i => i.Date >= prevStart && i.Date < prevEnd)
            .SumAsync(i => (double)i.ConvertedAmount);

        var prevExpenses = (decimal)await _db.Expenses
            .Where(e => e.Date >= prevStart && e.Date < prevEnd)
            .SumAsync(e => (double)e.ConvertedAmount);

        // Top expense category this month
        var topCategory = await _db.Expenses
            .Where(e => e.Date >= startDate && e.Date < endDate)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new { Name = g.Key, Total = g.Sum(e => (double)e.ConvertedAmount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefaultAsync();

        var txCount = await _db.Incomes.CountAsync(i => i.Date >= startDate && i.Date < endDate)
                    + await _db.Expenses.CountAsync(e => e.Date >= startDate && e.Date < endDate);

        return new DashboardSummaryDto
        {
            MonthlyIncome = monthlyIncome,
            MonthlyExpenses = monthlyExpenses,
            MonthlyDeductions = monthlyDeductions,
            MonthlySavings = monthlySavings,
            NetBalance = monthlyIncome - monthlyExpenses - monthlyDeductions - monthlySavings,
            ActiveProjects = activeProjects,
            CompaniesCount = companiesCount,
            CurrencyCode = defaultCurrency,
            UserDisplayName = userName,
            ProfileImagePath = profileImage,
            PreviousMonthIncome = prevIncome,
            PreviousMonthExpenses = prevExpenses,
            TopExpenseCategory = topCategory?.Name,
            TopExpenseCategoryAmount = (decimal)(topCategory?.Total ?? 0),
            TransactionCount = txCount
        };
    }

    public async Task<IReadOnlyList<MonthlyDataPoint>> GetMonthlyTrendsAsync(int year)
    {
        var result = new List<MonthlyDataPoint>();

        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var income = (decimal)await _db.Incomes
                .Where(i => i.Date >= startDate && i.Date < endDate)
                .SumAsync(i => (double)i.ConvertedAmount);

            var expenses = (decimal)await _db.Expenses
                .Where(e => e.Date >= startDate && e.Date < endDate)
                .SumAsync(e => (double)e.ConvertedAmount);

            result.Add(new MonthlyDataPoint
            {
                Month = startDate.ToString("MMM"),
                Income = income,
                Expenses = expenses
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<CategoryBreakdownItem>> GetExpenseByCategoryAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var breakdown = await _db.Expenses
            .Where(e => e.Date >= startDate && e.Date < endDate)
            .GroupBy(e => new { e.Category!.Name, e.Category.ColorHex })
            .Select(g => new CategoryBreakdownItem
            {
                Category = g.Key.Name,
                ColorHex = g.Key.ColorHex,
                Amount = (decimal)g.Sum(e => (double)e.ConvertedAmount)
            })
            .OrderByDescending(c => (double)c.Amount)
            .ToListAsync();

        var total = breakdown.Sum(b => b.Amount);
        foreach (var item in breakdown)
        {
            item.Percentage = total > 0 ? (double)(item.Amount / total * 100) : 0;
        }

        return breakdown;
    }

    public async Task<IReadOnlyList<DashboardSavingGoalDto>> GetSavingGoalsSummaryAsync()
    {
        var goals = await _db.SavingGoals
            .Where(g => !g.IsCompleted)
            .OrderByDescending(g => g.CreatedAt)
            .Take(5)
            .ToListAsync();

        var result = new List<DashboardSavingGoalDto>();
        foreach (var g in goals)
        {
            var saved = (decimal)await _db.SavingEntries
                .Where(e => e.SavingGoalId == g.Id)
                .SumAsync(e => (double)e.ConvertedAmount);

            result.Add(new DashboardSavingGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                TargetAmount = g.TargetAmount,
                SavedAmount = saved,
                ProgressPercentage = g.TargetAmount > 0 ? (double)(saved / g.TargetAmount * 100) : 0,
                CurrencyCode = g.CurrencyCode
            });
        }
        return result;
    }

    public async Task<IReadOnlyList<RecentTransactionDto>> GetRecentTransactionsAsync(int count = 10)
    {
        var recentIncomes = await _db.Incomes
            .OrderByDescending(i => i.Date)
            .Take(count)
            .Select(i => new RecentTransactionDto
            {
                Description = i.Description,
                Amount = i.Amount,
                CurrencyCode = i.CurrencyCode,
                Date = i.Date,
                IsIncome = true,
                CategoryOrSource = i.SourceType.ToString()
            })
            .ToListAsync();

        var recentExpenses = await _db.Expenses
            .OrderByDescending(e => e.Date)
            .Take(count)
            .Include(e => e.Category)
            .Select(e => new RecentTransactionDto
            {
                Description = e.Description,
                Amount = e.Amount,
                CurrencyCode = e.CurrencyCode,
                Date = e.Date,
                IsIncome = false,
                CategoryOrSource = e.Category != null ? e.Category.Name : null
            })
            .ToListAsync();

        return recentIncomes
            .Concat(recentExpenses)
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();
    }
}
