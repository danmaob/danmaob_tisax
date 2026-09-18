using DanmaobTisax.Application.Common;

namespace DanmaobTisax.Application.Auditing;

/// <summary>
/// Query service for retrieving audit logs (interface only).
/// </summary>
public interface IAuditLogQueryService
{
    /// <summary>
    /// Queries audit logs based on the provided filter.
    /// </summary>
    /// <param name="filter">The query filter containing search criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result containing matching audit logs.</returns>
    Task<PagedResult<AuditLogDto>> QueryAsync(AuditLogQueryFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the history (audit trail) for a specific entity.
    /// </summary>
    /// <param name="entityName">The entity type name.</param>
    /// <param name="entityId">The entity identifier, as a string (supports Guid/int/composite keys).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of audit records for the specified entity.</returns>
    Task<IReadOnlyList<AuditLogDto>> GetHistoryForEntityAsync(string entityName, string entityId, CancellationToken cancellationToken);
}
