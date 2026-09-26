namespace DanmaobTisax.Application.Tenants;

public sealed record TenantModuleStateDto(string ModuleCode, bool EnabledByPlan, bool? ExceptionIsEnabled, bool IsEnabled);
