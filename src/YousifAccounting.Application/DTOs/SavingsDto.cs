namespace YousifAccounting.Application.DTOs;

public class SavingGoalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal SavedAmount { get; set; }
    public double ProgressPercentage { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? TargetDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SavingGoalCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? TargetDate { get; set; }
}

public class SavingGoalUpdateDto : SavingGoalCreateDto
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; }
}

public class SavingEntryDto
{
    public int Id { get; set; }
    public int SavingGoalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}

public class SavingEntryCreateDto
{
    public int SavingGoalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
}
