using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Domain.Entities;

public sealed class AuditEntry : BaseEntity
{
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = null!;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
}
