using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Application.Tenants;

/// <summary>
/// Requisito de autorización para módulos específicos.
/// </summary>
public sealed class ModuleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Código del módulo requerido.
    /// </summary>
    public string ModuleCode { get; }

    /// <summary>
    /// Inicializa una nueva instancia de ModuleRequirement.
    /// </summary>
    /// <param name="moduleCode">El código del módulo.</param>
    public ModuleRequirement(string moduleCode)
    {
        ModuleCode = moduleCode;
    }
}
