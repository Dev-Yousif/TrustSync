using TrustSync.Domain.Enums;

namespace TrustSync.Application.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CompanyClientId { get; set; }
    public string? CompanyClientName { get; set; }
    public ProjectStatus Status { get; set; }
    public decimal AgreedAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CompletionPercentage { get; set; }
    public string? Notes { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Profit => ReceivedAmount - TotalExpenses;
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class ProjectCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int? CompanyClientId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public decimal AgreedAmount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CompletionPercentage { get; set; }
    public string? Notes { get; set; }
}

public class ProjectUpdateDto : ProjectCreateDto
{
    public int Id { get; set; }
    public decimal ReceivedAmount { get; set; }
}
