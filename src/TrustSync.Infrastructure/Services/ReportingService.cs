using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Infrastructure.Services;

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

            var income = (decimal)await _db.Incomes.Where(i => i.Date >= start && i.Date < end).SumAsync(i => (double)i.ConvertedAmount);
            var expenses = (decimal)await _db.Expenses.Where(e => e.Date >= start && e.Date < end).SumAsync(e => (double)e.ConvertedAmount);
            var deductions = (decimal)await _db.Deductions
                .Where(d => d.IsActive && d.StartDate <= end && (d.EndDate == null || d.EndDate >= start))
                .SumAsync(d => (double)d.ConvertedAmount);
            var savings = (decimal)await _db.SavingEntries.Where(s => s.Date >= start && s.Date < end).SumAsync(s => (double)s.ConvertedAmount);

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
            .Select(g => new IncomeBySourceItem { Source = g.Key.ToString(), Amount = (decimal)g.Sum(i => (double)i.ConvertedAmount) })
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
                Amount = (decimal)g.Sum(e => (double)e.ConvertedAmount)
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
                TotalExpenses = (decimal)p.Expenses.Sum(e => (double)e.ConvertedAmount),
                Profit = p.ReceivedAmount - (decimal)p.Expenses.Sum(e => (double)e.ConvertedAmount),
                ProfitMargin = p.ReceivedAmount > 0
                    ? (double)((p.ReceivedAmount - (decimal)p.Expenses.Sum(e => (double)e.ConvertedAmount)) / p.ReceivedAmount * 100)
                    : 0
            })
            .OrderByDescending(p => p.Profit)
            .ToListAsync();
    }

    public async Task<string> GetDefaultCurrencyCodeAsync()
    {
        return await _db.AppSettings
            .Where(s => s.Key == "DefaultCurrency")
            .Select(s => s.Value)
            .FirstOrDefaultAsync() ?? "USD";
    }

    public async Task<byte[]> ExportMonthlySummaryToPdfAsync(int year, string currencySymbol)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = await GetMonthlySummariesAsync(year);
        var incomeTotal = data.Sum(d => d.Income);
        var expenseTotal = data.Sum(d => d.Expenses);
        var deductionTotal = data.Sum(d => d.Deductions);
        var savingsTotal = data.Sum(d => d.Savings);
        var netTotal = data.Sum(d => d.Net);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text($"TrustSync — Financial Report {year}")
                        .FontSize(20).Bold().FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(4).Text($"Generated on {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(20).Column(col =>
                {
                    // Monthly Summary Table
                    col.Item().Text("Monthly Summary").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        // Header
                        table.Header(header =>
                        {
                            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Grey.Darken3).Padding(6).Text("Month").Style(headerStyle);
                            header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Income").Style(headerStyle);
                            header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Expenses").Style(headerStyle);
                            header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Deductions").Style(headerStyle);
                            header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Savings").Style(headerStyle);
                            header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Net").Style(headerStyle);
                        });

                        // Data rows
                        var rowIndex = 0;
                        foreach (var row in data)
                        {
                            var bg = rowIndex++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(6).Text(row.MonthName);
                            table.Cell().Background(bg).Padding(6).AlignRight().Text($"{currencySymbol}{row.Income:N2}").FontColor(Colors.Green.Darken2);
                            table.Cell().Background(bg).Padding(6).AlignRight().Text($"{currencySymbol}{row.Expenses:N2}").FontColor(Colors.Red.Darken1);
                            table.Cell().Background(bg).Padding(6).AlignRight().Text($"{currencySymbol}{row.Deductions:N2}").FontColor(Colors.Orange.Darken2);
                            table.Cell().Background(bg).Padding(6).AlignRight().Text($"{currencySymbol}{row.Savings:N2}");
                            table.Cell().Background(bg).Padding(6).AlignRight().Text($"{currencySymbol}{row.Net:N2}").Bold();
                        }

                        // Totals row
                        var totalStyle = TextStyle.Default.FontSize(10).Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(6).Text("Total").Style(totalStyle);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"{currencySymbol}{incomeTotal:N2}").Style(totalStyle).FontColor(Colors.Green.Darken2);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"{currencySymbol}{expenseTotal:N2}").Style(totalStyle).FontColor(Colors.Red.Darken1);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"{currencySymbol}{deductionTotal:N2}").Style(totalStyle).FontColor(Colors.Orange.Darken2);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"{currencySymbol}{savingsTotal:N2}").Style(totalStyle);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"{currencySymbol}{netTotal:N2}").Style(totalStyle);
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("TrustSync").Bold().FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span($" — Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
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
