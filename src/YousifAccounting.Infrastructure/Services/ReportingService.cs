using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class ReportingService : IReportingService
{
    private readonly AppDbContext _db;
    public ReportingService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<MonthlySummaryReport>> GetMonthlySummariesAsync(int year)
    {
        var result = new List<MonthlySummaryReport>();
        for (int month = 1; month <= 12; month++)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var income = (decimal)await _db.Incomes.Where(i => i.Date >= start && i.Date < end).SumAsync(i => (double)i.Amount);
            var expenses = (decimal)await _db.Expenses.Where(e => e.Date >= start && e.Date < end).SumAsync(e => (double)e.Amount);
            var deductions = (decimal)await _db.Deductions
                .Where(d => d.IsActive && d.StartDate <= end && (d.EndDate == null || d.EndDate >= start))
                .SumAsync(d => (double)d.Amount);
            var savings = (decimal)await _db.SavingEntries.Where(s => s.Date >= start && s.Date < end).SumAsync(s => (double)s.Amount);

            result.Add(new MonthlySummaryReport
            {
                Year = year, Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                Income = income, Expenses = expenses, Deductions = deductions,
                Savings = savings, Net = income - expenses - deductions - savings
            });
        }
        return result;
    }

    public async Task<IReadOnlyList<IncomeBySourceItem>> GetIncomeBySourceAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var items = await _db.Incomes
            .Where(i => i.Date >= start && i.Date < end)
            .GroupBy(i => i.SourceType)
            .Select(g => new IncomeBySourceItem { Source = g.Key.ToString(), Amount = (decimal)g.Sum(i => (double)i.Amount) })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        var total = items.Sum(i => i.Amount);
        foreach (var item in items)
            item.Percentage = total > 0 ? (double)(item.Amount / total * 100) : 0;

        return items;
    }

    public async Task<IReadOnlyList<CategoryBreakdownItem>> GetExpenseByCategoryAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var items = await _db.Expenses
            .Where(e => e.Date >= start && e.Date < end)
            .GroupBy(e => new { e.Category!.Name, e.Category.ColorHex })
            .Select(g => new CategoryBreakdownItem
            {
                Category = g.Key.Name, ColorHex = g.Key.ColorHex,
                Amount = (decimal)g.Sum(e => (double)e.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        var total = items.Sum(i => i.Amount);
        foreach (var item in items)
            item.Percentage = total > 0 ? (double)(item.Amount / total * 100) : 0;

        return items;
    }

    public async Task<IReadOnlyList<ProjectProfitabilityItem>> GetProjectProfitabilityAsync()
    {
        return await _db.Projects
            .Include(p => p.CompanyClient)
            .Select(p => new ProjectProfitabilityItem
            {
                ProjectName = p.Name,
                CompanyName = p.CompanyClient != null ? p.CompanyClient.Name : null,
                AgreedAmount = p.AgreedAmount,
                ReceivedAmount = p.ReceivedAmount,
                TotalExpenses = (decimal)p.Expenses.Sum(e => (double)e.Amount),
                Profit = p.ReceivedAmount - (decimal)p.Expenses.Sum(e => (double)e.Amount),
                ProfitMargin = p.ReceivedAmount > 0
                    ? (double)((p.ReceivedAmount - (decimal)p.Expenses.Sum(e => (double)e.Amount)) / p.ReceivedAmount * 100)
                    : 0
            })
            .OrderByDescending(p => p.Profit)
            .ToListAsync();
    }

    public async Task<string> ExportMonthlySummaryToCsvAsync(int year)
    {
        var data = await GetMonthlySummariesAsync(year);
        var sb = new StringBuilder();
        sb.AppendLine("Month,Income,Expenses,Deductions,Savings,Net");
        foreach (var row in data)
            sb.AppendLine($"{row.MonthName},{row.Income:F2},{row.Expenses:F2},{row.Deductions:F2},{row.Savings:F2},{row.Net:F2}");
        return sb.ToString();
    }
}
