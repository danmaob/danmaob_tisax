using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Auditing;

public partial class PlatformAuditQueryService : IPlatformAuditQueryService
{
    public async Task<PagedResult<PlatformAuditLogDto>> QueryAsync(PlatformAuditLogQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = BuildQuery(filter);
        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var pageNumber = Math.Max(filter.PageNumber, 1);
        var rows = await query.OrderByDescending(a => a.PerformedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select(ToDto).ToList();
        return new PagedResult<PlatformAuditLogDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<IReadOnlyList<PlatformAuditLogDto>> ExportAsync(PlatformAuditLogQueryFilter filter, CancellationToken cancellationToken)
    {
        var rows = await BuildQuery(filter).OrderByDescending(a => a.PerformedAtUtc).Take(10000).ToListAsync(cancellationToken);
        return rows.Select(ToDto).ToList();
    }
}
