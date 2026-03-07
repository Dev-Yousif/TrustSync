using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Domain.Entities;

public sealed class Income : BaseEntity, ISoftDeletable
{
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; }
    public IncomeSourceType SourceType { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Received;
    public int? CompanyClientId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Currency Currency { get; set; } = null!;
    public CompanyClient? CompanyClient { get; set; }
    public Project? Project { get; set; }
}
