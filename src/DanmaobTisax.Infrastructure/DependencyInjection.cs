using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.MultiTenancy;
using DanmaobTisax.Infrastructure.Persistence;
using DanmaobTisax.Infrastructure.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));

        // Tenant resolution depends only on static configuration, so it can be a singleton.
        services.AddSingleton<ICurrentTenantProvider, CurrentTenantProvider>();

        // Register the CurrentUserService via DI (it will be injected with user from auth middleware)
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register AuthenticationService
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPlatformAuthenticationService, PlatformAuthenticationService>();

        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();

        services.AddScoped<IModuleAccessEvaluator, ModuleAccessEvaluator>();
        services.AddScoped<ITenantStatusEvaluator, TenantStatusEvaluator>();

        services.AddScoped<ITenantAdministrationService, TenantAdministrationService>();
        services.AddScoped<IPlanAdministrationService, PlanAdministrationService>();

        // Register authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ModuleAuthorizationHandler>();

        // Register authorization policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<DanmaobTisaxDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        return services;
    }
}
