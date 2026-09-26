namespace DanmaobTisax.Application.Tenants;

public interface ITenantModuleAdministrationService
{
    Task<IReadOnlyList<TenantModuleStateDto>?> GetModulesAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<TenantModuleOperationResult> SetExceptionAsync(Guid tenantId, string moduleCode, string state, CancellationToken cancellationToken);
}
