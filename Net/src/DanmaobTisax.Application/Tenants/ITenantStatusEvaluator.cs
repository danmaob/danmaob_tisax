namespace DanmaobTisax.Application.Tenants;

public interface ITenantStatusEvaluator
{
    /// <summary>
    /// Evaluates whether a tenant is blocked based on its status.
    /// Returns true only when a tenant with that id exists AND its status is not Active
    /// (that is, it is Suspended or Deactivated). Returns false when the tenant is Active
    /// and also returns false when no tenant row exists.
    /// </summary>
    Task<bool> IsTenantBlockedAsync(Guid tenantId, CancellationToken cancellationToken);
}
