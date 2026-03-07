using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Domain.Entities;

public sealed class Deduction : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ConvertedAmount { get; set; }
    public string ConvertedCurrencyCode { get; set; } = "USD";
    public decimal ExchangeRateUsed { get; set; } = 1m;
    public DeductionType Type { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Monthly;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Currency Currency { get; set; } = null!;
}
