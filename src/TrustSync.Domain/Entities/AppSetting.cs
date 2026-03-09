namespace TrustSync.Domain.Entities;

public sealed class AppSetting
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
