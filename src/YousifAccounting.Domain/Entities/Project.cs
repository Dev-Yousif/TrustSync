using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Domain.Entities;

public sealed class Project : BaseEntity, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? CompanyClientId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public decimal AgreedAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CompletionPercentage { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public CompanyClient? CompanyClient { get; set; }
    public Currency Currency { get; set; } = null!;
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
