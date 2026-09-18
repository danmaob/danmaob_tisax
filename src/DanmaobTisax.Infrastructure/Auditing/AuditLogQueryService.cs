using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Auditing;

/// <summary>
/// Implementation of the audit log query service.
/// Always scopes every query to the current tenant.
/// </summary>
public class AuditLogQueryService : IAuditLogQueryService
{
    private readonly DanmaobTisaxDbContext _context;
    private readonly ICurrentTenantProvider _tenantProvider;

    public AuditLogQueryService(
        DanmaobTisaxDbContext context,
        ICurrentTenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PagedResult<AuditLogDto>> QueryAsync(AuditLogQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == _tenantProvider.CurrentTenantId);

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(a => a.EntityName == filter.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityId))
        {
            query = query.Where(a => a.EntityId == filter.EntityId);
        }

        if (filter.PerformedByUserId.HasValue)
        {
            query = query.Where(a => a.PerformedByUserId == filter.PerformedByUserId.Value);
        }

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(a => a.PerformedAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(a => a.PerformedAtUtc <= filter.ToUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var pageNumber = Math.Max(filter.PageNumber, 1);

        var pagedItems = await query
            .OrderByDescending(a => a.PerformedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action.ToString(),
                PerformedByUserId = a.PerformedByUserId,
                PerformedByDisplayName = a.PerformedByDisplayName,
                PerformedAtUtc = a.PerformedAtUtc,
                OldValuesJson = a.OldValuesJson,
                NewValuesJson = a.NewValuesJson,
                ChangedColumnsJson = a.ChangedColumnsJson
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(pagedItems, totalCount, pageNumber, pageSize);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetHistoryForEntityAsync(string entityName, string entityId, CancellationToken cancellationToken)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == _tenantProvider.CurrentTenantId)
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderBy(a => a.PerformedAtUtc)
            .Take(500)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action.ToString(),
                PerformedByUserId = a.PerformedByUserId,
                PerformedByDisplayName = a.PerformedByDisplayName,
                PerformedAtUtc = a.PerformedAtUtc,
                OldValuesJson = a.OldValuesJson,
                NewValuesJson = a.NewValuesJson,
                ChangedColumnsJson = a.ChangedColumnsJson
            })
            .ToListAsync(cancellationToken);
    }
}
