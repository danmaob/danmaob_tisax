using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class PlatformAdminBootstrapTests : IClassFixture<BootstrapWebApplicationFactory>
{
    private readonly BootstrapWebApplicationFactory _factory;

    public PlatformAdminBootstrapTests(BootstrapWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PlatformBootstrap_CreatesPlatformAdministratorThatCoexistsWithTenantAdministrator()
    {
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);
        await PlatformAdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var administratorCount = await context.PlatformAdministrators.CountAsync(a => a.Email == "bootstrap-admin@example.com");
        var tenantUserCount = await context.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && u.Email == "bootstrap-admin@example.com");
        Assert.Equal(1, administratorCount);
        Assert.Equal(1, tenantUserCount);
    }

    [Fact]
    public async Task PlatformBootstrap_RunTwice_DoesNotDuplicatePlatformAdministrator()
    {
        await PlatformAdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);
        await PlatformAdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var administratorCount = await context.PlatformAdministrators.CountAsync(a => a.Email == "bootstrap-admin@example.com");
        Assert.Equal(1, administratorCount);
    }

    [Fact]
    public async Task AdminBootstrap_TenantAdministratorRoleHasNoPlatformPermissions()
    {
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var adminRole = await context.Roles.IgnoreQueryFilters().FirstAsync(r => r.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && r.Name == "Administrator");
        var platformPermissionIds = await context.Permissions.Where(p => p.Module == "Platform").Select(p => p.Id).ToListAsync();
        var platformGrantCount = await context.RolePermissions.CountAsync(rp => rp.RoleId == adminRole.Id && platformPermissionIds.Contains(rp.PermissionId));
        Assert.Equal(0, platformGrantCount);
    }
}
