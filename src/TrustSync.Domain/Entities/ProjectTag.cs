namespace TrustSync.Domain.Entities;

public sealed class ProjectTag
{
    public int ProjectId { get; set; }
    public int TagId { get; set; }

    public Project Project { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
