namespace DanmaobTisax.Application.Interfaces;

public enum MultiTenancyMode
{
    SingleTenant,
    MultiTenant
}

/// <summary>
/// Resolves the tenant the current unit of work belongs to. See
/// ADR-0002 — `MultiTenant` mode is intentionally not implemented until
/// authentication (TS-00-3) and per-request tenant/module evaluation
/// (US-20-2) exist.
/// </summary>
public interface ICurrentTenantProvider
{
    MultiTenancyMode Mode { get; }
    Guid? CurrentTenantId { get; }
}