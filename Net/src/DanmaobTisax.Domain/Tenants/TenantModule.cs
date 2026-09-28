using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Common;

namespace DanmaobTisax.Domain.Tenants;

public class TenantModule : BaseEntity, ITenantOwned, IAuditable
{
    public Guid TenantId { get; set; }

    public string ModuleCode { get; private set; } = string.Empty;

    public bool? IsEnabled { get; private set; }

    protected TenantModule()
    {
    }

    public TenantModule(Guid tenantId, string moduleCode, bool isEnabled)
    {
        if (Guid.Empty.Equals(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(moduleCode))
        {
            throw new ArgumentException("Module code is required.", nameof(moduleCode));
        }

        if (moduleCode.Length > 100)
        {
            throw new ArgumentException("Module code must not exceed 100 characters.", nameof(moduleCode));
        }

        TenantId = tenantId;
        ModuleCode = moduleCode;
        IsEnabled = isEnabled;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void ClearException()
    {
        IsEnabled = null;
    }
}
