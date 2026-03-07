namespace YousifAccounting.Application.DTOs;

public class DashboardSummaryDto
{
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyDeductions { get; set; }
    public decimal MonthlySavings { get; set; }
    public decimal NetBalance { get; set; }
    public int ActiveProjects { get; set; }
    public int CompaniesCount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class MonthlyDataPoint
{
    public required string Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

public class CategoryBreakdownItem
{
    public required string Category { get; set; }
    public string? ColorHex { get; set; }
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

public class RecentTransactionDto
{
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public string? CategoryOrSource { get; set; }
}
