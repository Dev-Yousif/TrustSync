using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Application.DTOs;

public class DeductionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DeductionType Type { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeductionCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DeductionType Type { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class DeductionUpdateDto : DeductionCreateDto
{
    public int Id { get; set; }
}
