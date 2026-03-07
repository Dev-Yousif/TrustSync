using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Application.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColorHex { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public int? CompanyClientId { get; set; }
    public string? CompanyClientName { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseCreateDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; } = DateTime.Today;
    public int CategoryId { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public int? CompanyClientId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public string? Notes { get; set; }
}

public class ExpenseUpdateDto : ExpenseCreateDto
{
    public int Id { get; set; }
}

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
    public bool IsDefault { get; set; }
}
