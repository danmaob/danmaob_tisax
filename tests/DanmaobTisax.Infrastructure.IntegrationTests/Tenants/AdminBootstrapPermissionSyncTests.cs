using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class AdminBootstrapPermissionSyncTests : IClassFixture<BootstrapWebApplicationFactory>
{
    private readonly BootstrapWebApplicationFactory _factory;

    public AdminBootstrapPermissionSyncTests(BootstrapWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Bootstrap_RunAgain_RestoresMissingAdministratorPermissionsWithoutDuplicatingTheAdministrator()
    {
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
            var adminRole = await context.Roles.IgnoreQueryFilters().FirstAsync(r => r.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && r.Name == "Administrator");
            var oneGrant = await context.RolePermissions.FirstAsync(rp => rp.RoleId == adminRole.Id);
            context.RolePermissions.Remove(oneGrant);
            await context.SaveChangesAsync();
        }

        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
            var adminRole = await context.Roles.IgnoreQueryFilters().FirstAsync(r => r.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && r.Name == "Administrator");
            var permissionCount = await context.Permissions.CountAsync();
            var grantCount = await context.RolePermissions.CountAsync(rp => rp.RoleId == adminRole.Id);
            var administratorCount = await context.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == BootstrapWebApplicationFactory.BootstrapTenantId && u.Email == "bootstrap-admin@example.com");

            Assert.True(permissionCount > 0);
            Assert.Equal(permissionCount, grantCount);
            Assert.Equal(1, administratorCount);
        }
    }
}
