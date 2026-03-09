using TrustSync.Domain.Enums;

namespace TrustSync.Application.DTOs;

public class CompanyClientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CompanyType Type { get; set; }
    public EngagementType EngagementType { get; set; }
    public CompanyStatus Status { get; set; }
    public string? ContactEmail { get; set; }
    public string? Notes { get; set; }
    public string DefaultCurrencyCode { get; set; } = "USD";
    public int ProjectsCount { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CompanyClientCreateDto
{
    public string Name { get; set; } = string.Empty;
    public CompanyType Type { get; set; }
    public EngagementType EngagementType { get; set; }
    public CompanyStatus Status { get; set; } = CompanyStatus.Active;
    public string? ContactEmail { get; set; }
    public string? Notes { get; set; }
    public string DefaultCurrencyCode { get; set; } = "USD";
}

public class CompanyClientUpdateDto : CompanyClientCreateDto
{
    public int Id { get; set; }
}
