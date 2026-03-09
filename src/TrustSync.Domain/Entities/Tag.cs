using TrustSync.Domain.Common;

namespace TrustSync.Domain.Entities;

public sealed class Tag : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? ColorHex { get; set; }

    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
