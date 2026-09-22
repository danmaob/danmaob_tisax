using DanmaobTisax.Application.Identity;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class TenantStatusWebApplicationFactory : PlatformAdminWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(TenantStatusProbeController).Assembly);
        });
    }

    public string CreateTokenForTenant(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var provider = scope.ServiceProvider;
        var jwtService = provider.GetRequiredService<IJwtTokenService>();
        var token = jwtService.GenerateAccessToken(
            Guid.NewGuid(),
            tenantId,
            "probe@example.com",
            Array.Empty<string>(),
            Array.Empty<string>());

        return token.Token;
    }

    private async Task<Tenant> GetOrCreateTenantAsync(DanmaobTisaxDbContext context, Guid tenantId)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant is null)
        {
            tenant = new Tenant("Probe-" + tenantId) { Id = tenantId };
            context.Tenants.Add(tenant);
        }

        return tenant;
    }

    public async Task EnsureTenantRowAsync(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var tenant = await GetOrCreateTenantAsync(context, tenantId);
        await context.SaveChangesAsync();
    }

    public async Task SuspendTenantRowAsync(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var tenant = await GetOrCreateTenantAsync(context, tenantId);
        tenant.Suspend();
        await context.SaveChangesAsync();
    }

    public async Task ReactivateTenantRowAsync(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var tenant = await GetOrCreateTenantAsync(context, tenantId);
        tenant.Reactivate();
        await context.SaveChangesAsync();
    }

    public async Task DeactivateTenantRowAsync(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var tenant = await GetOrCreateTenantAsync(context, tenantId);
        tenant.Deactivate();
        await context.SaveChangesAsync();
    }
}
