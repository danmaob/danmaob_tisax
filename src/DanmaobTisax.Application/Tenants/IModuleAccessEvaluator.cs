using System;

namespace DanmaobTisax.Application.Tenants;

/// <summary>
/// Evaluador que determina si un módulo es habilitado para un tenant específico.
/// </summary>
public interface IModuleAccessEvaluator
{
    /// <summary>
    /// Determines whether the module is enabled for the tenant. A TenantModule row for that tenant and module is an explicit exception and decides the result on its own (enabled or disabled). Without such a row, the result is true only when the tenant exists and its plan has that module enabled in the module-plan matrix. Implementations must evaluate on every call and must not cache.
    /// </summary>
    /// <param name="tenantId">El ID del tenant para verificar el acceso al módulo.</param>
    /// <param name="moduleCode">El código del módulo a verificar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>True si el módulo está habilitado para el tenant, false en caso contrario.</returns>
    Task<bool> IsModuleEnabledAsync(Guid tenantId, string moduleCode, CancellationToken cancellationToken);
}
