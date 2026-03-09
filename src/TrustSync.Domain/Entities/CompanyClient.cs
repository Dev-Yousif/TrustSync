using TrustSync.Domain.Common;
using TrustSync.Domain.Enums;

namespace TrustSync.Domain.Entities;

public sealed class CompanyClient : BaseEntity, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public CompanyType Type { get; set; }
    public EngagementType EngagementType { get; set; }
    public CompanyStatus Status { get; set; } = CompanyStatus.Active;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string DefaultCurrencyCode { get; set; } = "USD";
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Currency DefaultCurrency { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
