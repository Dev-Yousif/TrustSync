using YousifAccounting.Domain.Common;

namespace YousifAccounting.Domain.Entities;

public sealed class SavingEntry : BaseEntity, ISoftDeletable
{
    public int SavingGoalId { get; set; }
    public decimal Amount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public string ConvertedCurrencyCode { get; set; } = "USD";
    public decimal ExchangeRateUsed { get; set; } = 1m;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public SavingGoal SavingGoal { get; set; } = null!;
}
