using DanmaobTisax.Domain.Common;

namespace DanmaobTisax.Infrastructure.Auditing;

public static class AuditTenantResolver
{
    public static Guid? Resolve(object entity)
    {
        if (entity is ITenantOwned tenantOwned)
        {
            return tenantOwned.TenantId;
        }

        return null;
    }
}