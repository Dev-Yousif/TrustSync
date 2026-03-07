using YousifAccounting.Domain.Common;

namespace YousifAccounting.Domain.Entities;

public sealed class Tag : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? ColorHex { get; set; }

    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
