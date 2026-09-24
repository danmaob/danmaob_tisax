namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

using DanmaobTisax.Application.Identity;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class PlanGateWebApplicationFactory : PlatformAdminWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services => { services.AddControllers().AddApplicationPart(typeof(PlanGateProbeController).Assembly); });
    }

    public string CreateTokenForTenant(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var result = jwtTokenService.GenerateAccessToken(
            Guid.NewGuid(),
            tenantId,
            "probe@example.com",
            Array.Empty<string>(),
            Array.Empty<string>());

        return result.Token;
    }

    public async Task SetTenantModuleAsync(Guid tenantId, string moduleCode, bool enabled)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var existing = await context.TenantModules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.ModuleCode == moduleCode);

        if (existing is null)
        {
            context.TenantModules.Add(new TenantModule(tenantId, moduleCode, enabled));
        }
        else if (enabled)
        {
            existing.Enable();
        }
        else
        {
            existing.Disable();
        }

        await context.SaveChangesAsync();
    }
}
