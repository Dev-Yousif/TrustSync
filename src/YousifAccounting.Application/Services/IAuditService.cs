using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Application.Services;

public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityType, int? entityId = null, string? details = null);
}
