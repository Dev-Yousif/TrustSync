namespace YousifAccounting.Application.DTOs;

public class MonthlySummaryReport
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Deductions { get; set; }
    public decimal Savings { get; set; }
    public decimal Net { get; set; }
}

public class IncomeBySourceItem
{
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

public class ProjectProfitabilityItem
{
    public string ProjectName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public decimal AgreedAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Profit { get; set; }
    public double ProfitMargin { get; set; }
}
