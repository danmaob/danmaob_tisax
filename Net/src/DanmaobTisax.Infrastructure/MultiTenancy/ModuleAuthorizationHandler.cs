using DanmaobTisax.Application.Tenants;
using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
{
    private readonly IModuleAccessEvaluator _evaluator;

    public ModuleAuthorizationHandler(IModuleAccessEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return;
        }

        var tenantClaimValue = context.User.FindFirst("tenant")?.Value;
        if (Guid.TryParse(tenantClaimValue, out var tenantId))
        {
            var isEnabled = await _evaluator.IsModuleEnabledAsync(tenantId, requirement.ModuleCode, CancellationToken.None);

            if (isEnabled)
            {
                context.Succeed(requirement);
            }
        }
    }
}
