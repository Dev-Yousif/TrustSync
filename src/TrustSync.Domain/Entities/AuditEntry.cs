using TrustSync.Domain.Common;
using TrustSync.Domain.Enums;

namespace TrustSync.Domain.Entities;

public sealed class AuditEntry : BaseEntity
{
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = null!;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
}
