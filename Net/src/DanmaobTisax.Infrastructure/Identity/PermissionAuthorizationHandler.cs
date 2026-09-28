using DanmaobTisax.Application.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Infrastructure.Identity;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (requirement.PermissionCode.StartsWith("Platform.", StringComparison.Ordinal) == true)
        {
            var principalType = context.User.FindFirst(PlatformClaims.PrincipalTypeClaim)?.Value;
            if (principalType != PlatformClaims.PlatformPrincipalType)
            {
                return Task.CompletedTask;
            }
        }

        foreach (var permClaim in context.User.FindAll("perm"))
        {
            if (permClaim.Value == requirement.PermissionCode)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
