using DanmaobTisax.Application.Identity;
using DanmaobTisax.Domain.Interfaces;
using DanmaobTisax.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the CurrentUserService via DI (it will be injected with user from auth middleware)
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register AuthenticationService
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Register authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Register authorization policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }
}
