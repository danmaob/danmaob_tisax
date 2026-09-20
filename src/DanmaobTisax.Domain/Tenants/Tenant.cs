namespace DanmaobTisax.Domain.Tenants;

using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Exceptions;

public class Tenant : BaseEntity, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public TenantStatus Status { get; private set; } = TenantStatus.Active;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Required by EF Core for materialization.</summary>
    protected Tenant()
    {
    }

    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        Name = name;
    }

    public void Suspend()
    {
        if (Status != TenantStatus.Active)
        {
            throw new InvalidTenantStatusTransitionException(Status, "suspend");
        }

        Status = TenantStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status != TenantStatus.Suspended)
        {
            throw new InvalidTenantStatusTransitionException(Status, "reactivate");
        }

        Status = TenantStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == TenantStatus.Deactivated)
        {
            throw new InvalidTenantStatusTransitionException(Status, "deactivate");
        }

        Status = TenantStatus.Deactivated;
    }
}
