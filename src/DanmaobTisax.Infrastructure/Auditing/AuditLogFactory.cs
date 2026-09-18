using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Auditing;

namespace DanmaobTisax.Infrastructure.Auditing;

public static class AuditLogFactory
{
    public static AuditLog Build(
        object entity,
        string entityName,
        string entityId,
        AuditAction action,
        ICurrentUserService currentUserService,
        string? oldValuesJson,
        string? newValuesJson,
        string? changedColumnsJson)
    {
        var tenantId = AuditTenantResolver.Resolve(entity);

        return new AuditLog(
            tenantId,
            entityName,
            entityId,
            action,
            currentUserService.UserId,
            currentUserService.DisplayName,
            DateTime.UtcNow,
            oldValuesJson,
            newValuesJson,
            changedColumnsJson);
    }
}