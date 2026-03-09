using TrustSync.Application.Services;
using TrustSync.Domain.Entities;
using TrustSync.Domain.Enums;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db) => _db = db;

    public async Task LogAsync(AuditAction action, string entityType, int? entityId = null, string? details = null)
    {
        _db.AuditEntries.Add(new AuditEntry
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        });
        await _db.SaveChangesAsync();
    }
}
