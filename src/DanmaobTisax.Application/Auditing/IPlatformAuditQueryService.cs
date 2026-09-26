using DanmaobTisax.Application.Common;

namespace DanmaobTisax.Application.Auditing;

public interface IPlatformAuditQueryService
{
    Task<PagedResult<PlatformAuditLogDto>> QueryAsync(PlatformAuditLogQueryFilter filter, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlatformAuditLogDto>> ExportAsync(PlatformAuditLogQueryFilter filter, CancellationToken cancellationToken);
}
