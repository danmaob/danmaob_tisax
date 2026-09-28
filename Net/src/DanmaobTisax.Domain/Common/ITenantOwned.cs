namespace DanmaobTisax.Domain.Common;

/// <summary>
/// Marks an entity as belonging to a specific tenant. Any entity implementing
/// this interface is automatically isolated by tenant via a global EF Core
/// query filter applied by DanmaobTisaxDbContext — no extra wiring required.
/// See ADR-0002.
/// </summary>
public interface ITenantOwned
{
    Guid TenantId { get; set; }
}