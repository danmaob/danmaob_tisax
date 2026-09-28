using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Auditing;

public partial class PlatformAuditQueryService
{
    private readonly DanmaobTisaxDbContext _context;

    public PlatformAuditQueryService(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    private IQueryable<AuditLog> BuildQuery(PlatformAuditLogQueryFilter filter)
    {
        var query = _context.AuditLogs.AsNoTracking().Where(a => PlatformAuditEntities.Names.Contains(a.EntityName));

        if (filter.TenantId.HasValue == true)
        {
            var tenantId = filter.TenantId.Value;
            var tenantIdText = tenantId.ToString();
            query = query.Where(a => (a.EntityName == "Tenant" && a.EntityId == tenantIdText) || (a.EntityName == "TenantModule" && a.TenantId == tenantId));
        }

        if (string.IsNullOrWhiteSpace(filter.EntityName) == false)
        {
            var entityName = filter.EntityName;
            query = query.Where(a => a.EntityName == entityName);
        }

        if (filter.Action.HasValue == true)
        {
            var action = filter.Action.Value;
            query = query.Where(a => a.Action == action);
        }

        if (filter.PerformedByUserId.HasValue == true)
        {
            var performedByUserId = filter.PerformedByUserId.Value;
            query = query.Where(a => a.PerformedByUserId == performedByUserId);
        }

        if (filter.FromUtc.HasValue == true)
        {
            var fromUtc = filter.FromUtc.Value;
            query = query.Where(a => a.PerformedAtUtc >= fromUtc);
        }

        if (filter.ToUtc.HasValue == true)
        {
            var toUtc = filter.ToUtc.Value;
            query = query.Where(a => a.PerformedAtUtc <= toUtc);
        }

        return query;
    }

    private static PlatformAuditLogDto ToDto(AuditLog a)
    {
        Guid? affectedTenantId = null;

        if (a.EntityName == "Tenant" && Guid.TryParse(a.EntityId, out var parsedTenantId) == true)
        {
            affectedTenantId = parsedTenantId;
        }
        else if (a.EntityName == "TenantModule")
        {
            affectedTenantId = a.TenantId;
        }

        return new PlatformAuditLogDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            AffectedTenantId = affectedTenantId,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            Action = a.Action.ToString(),
            PerformedByUserId = a.PerformedByUserId,
            PerformedByDisplayName = a.PerformedByDisplayName,
            PerformedAtUtc = a.PerformedAtUtc,
            OldValuesJson = a.OldValuesJson,
            NewValuesJson = a.NewValuesJson,
            ChangedColumnsJson = a.ChangedColumnsJson
        };
    }
}
