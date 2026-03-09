using TrustSync.Domain.Enums;

namespace TrustSync.Application.DTOs;

public class IncomeDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; }
    public IncomeSourceType SourceType { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int? CompanyClientId { get; set; }
    public string? CompanyClientName { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IncomeCreateDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; } = DateTime.Today;
    public IncomeSourceType SourceType { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Received;
    public int? CompanyClientId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public string? Notes { get; set; }
}

public class IncomeUpdateDto : IncomeCreateDto
{
    public int Id { get; set; }
}
