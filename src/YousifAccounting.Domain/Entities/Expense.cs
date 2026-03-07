using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Domain.Entities;

public sealed class Expense : BaseEntity, ISoftDeletable
{
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ConvertedAmount { get; set; }
    public string ConvertedCurrencyCode { get; set; } = "USD";
    public decimal ExchangeRateUsed { get; set; } = 1m;
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public ExpenseType ExpenseType { get; set; } = ExpenseType.Personal;
    public int? CompanyClientId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Currency Currency { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
    public CompanyClient? CompanyClient { get; set; }
    public Project? Project { get; set; }
}
