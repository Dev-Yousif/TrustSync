using TrustSync.Domain.Enums;

namespace TrustSync.Application.Services;

public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityType, int? entityId = null, string? details = null);
}
