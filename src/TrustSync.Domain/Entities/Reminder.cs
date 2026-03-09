using TrustSync.Domain.Common;
using TrustSync.Domain.Enums;

namespace TrustSync.Domain.Entities;

public sealed class Reminder : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsSystem { get; set; }
    public RepeatType RepeatType { get; set; } = RepeatType.Daily;
    public int? CustomIntervalMinutes { get; set; }
    public TimeOnly TimeOfDay { get; set; } = new(20, 0);
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public DateTime? NextFireAt { get; set; }
    public DateTime? LastFiredAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
