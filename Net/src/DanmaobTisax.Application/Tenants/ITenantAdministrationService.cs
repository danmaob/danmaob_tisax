using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Common;

namespace DanmaobTisax.Application.Tenants;

public interface ITenantAdministrationService
{
    /// <summary>
    /// Creates a new tenant with the specified name.
    /// Outcomes: Succeeded (Value contains the created tenant), InvalidName, NameAlreadyExists.
    /// </summary>
    Task<TenantOperationResult> CreateAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a tenant by its identifier.
    /// Returns null when the tenant does not exist.
    /// </summary>
    Task<TenantDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Suspends the specified tenant.
    /// Outcomes: Succeeded, NotFound, InvalidStatusTransition.
    /// </summary>
    Task<TenantOperationResult> SuspendAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Reactivates a suspended tenant.
    /// Outcomes: Succeeded, NotFound, InvalidStatusTransition.
    /// </summary>
    Task<TenantOperationResult> ReactivateAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Deactivates the specified tenant. Logical deactivation preserves all data.
    /// Outcomes: Succeeded, NotFound, InvalidStatusTransition.
    /// </summary>
    Task<TenantOperationResult> DeactivateAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the audit history for a tenant.
    /// Returns null when the tenant does not exist; otherwise returns that tenant's audit entries in chronological order.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>?> GetAuditHistoryAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Assigns a different plan to the tenant. Existing tenant data is never deleted. Outcomes: Succeeded, NotFound, PlanNotFound, PlanInactive.</summary>
    Task<TenantOperationResult> ChangePlanAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default);

    /// <summary>Returns tenants ordered by Name then Id, optionally filtered by Status and by a case-insensitive Name fragment, paged (page size clamped to 1..200, page number at least 1).</summary>
    Task<PagedResult<TenantDto>> ListAsync(TenantQueryFilter filter, CancellationToken cancellationToken);
}
