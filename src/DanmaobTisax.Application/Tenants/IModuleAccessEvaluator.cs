using System;

namespace DanmaobTisax.Application.Tenants;

/// <summary>
/// Evaluador que determina si un módulo es habilitado para un tenant específico.
/// </summary>
public interface IModuleAccessEvaluator
{
    /// <summary>
    /// Verifica si el módulo está habilitado para el tenant especificado.
    /// Devuelve true solo cuando existe una entrada explícita para ese tenant y módulo que está activada.
    /// Devuelve false cuando la entrada está deshabilitada o no existe.
    /// Las implementaciones deben evaluar en cada llamada y no deben cachear.
    /// </summary>
    /// <param name="tenantId">El ID del tenant para verificar el acceso al módulo.</param>
    /// <param name="moduleCode">El código del módulo a verificar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>True si el módulo está habilitado para el tenant, false en caso contrario.</returns>
    Task<bool> IsModuleEnabledAsync(Guid tenantId, string moduleCode, CancellationToken cancellationToken);
}
