using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class AdminBootstrapPlatformCleanupTests : IClassFixture<BootstrapWebApplicationFactory>
{
    private readonly BootstrapWebApplicationFactory _factory;

    public AdminBootstrapPlatformCleanupTests(BootstrapWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AdminBootstrap_ExistingAdministratorWithPlatformPermissions_RemovesThem()
    {
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);

        using (var seedScope = _factory.Services.CreateScope())
        {
            var seedContext = seedScope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
            var seedRole = await seedContext.Roles.IgnoreQueryFilters().FirstAsync(r => r.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && r.Name == "Administrator");
            var platformPermissions = await seedContext.Permissions.Where(p => p.Module == "Platform").ToListAsync();
            Assert.True(platformPermissions.Count > 0);
            foreach (var platformPermission in platformPermissions)
            {
                seedContext.RolePermissions.Add(new RolePermission(seedRole.Id, platformPermission.Id));
            }
            await seedContext.SaveChangesAsync();
        }

        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var adminRole = await verifyContext.Roles.IgnoreQueryFilters().FirstAsync(r => r.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && r.Name == "Administrator");
        var platformPermissionIds = await verifyContext.Permissions.Where(p => p.Module == "Platform").Select(p => p.Id).ToListAsync();
        var platformGrantCount = await verifyContext.RolePermissions.CountAsync(rp => rp.RoleId == adminRole.Id && platformPermissionIds.Contains(rp.PermissionId));
        Assert.Equal(0, platformGrantCount);
    }
}
