using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Tenants;
using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Infrastructure.Identity;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => Task.FromResult<AuthorizationPolicy>(new AuthorizationPolicy(new List<IAuthorizationRequirement>(), new List<string>()));
    
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);
    
    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Module:"))
        {
            var moduleCode = policyName.Substring("Module:".Length);
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new ModuleRequirement(moduleCode))
                .Build();
        }

        if (!policyName.StartsWith("Permission:"))
        {
            return null;
        }
        
        var permissionCode = policyName.Substring("Permission:".Length);
        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permissionCode))
            .Build();
    }
}
