using YousifAccounting.Domain.Common;

namespace YousifAccounting.Domain.Entities;

public sealed class SavingGoal : BaseEntity, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? TargetDate { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Currency Currency { get; set; } = null!;
    public ICollection<SavingEntry> Entries { get; set; } = new List<SavingEntry>();

    public decimal SavedAmount => Entries?.Where(e => !e.IsDeleted).Sum(e => e.Amount) ?? 0;
    public decimal ProgressPercentage => TargetAmount > 0 ? Math.Min(100, SavedAmount / TargetAmount * 100) : 0;
}
