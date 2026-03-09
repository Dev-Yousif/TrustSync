using TrustSync.Domain.Common;

namespace TrustSync.Domain.Entities;

public sealed class ExpenseCategory : BaseEntity, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
