using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class InstallationTenantBootstrapTests : IClassFixture<BootstrapWebApplicationFactory>
{
    private readonly BootstrapWebApplicationFactory _factory;

    public InstallationTenantBootstrapTests(BootstrapWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Bootstrap_CreatesInstallationTenantWithFreePlan()
    {
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Id == BootstrapWebApplicationFactory.BootstrapTenantId);

        Assert.NotNull(tenant);
        Assert.Equal(BootstrapWebApplicationFactory.BootstrapTenantName, tenant.Name);
        Assert.Equal(PlanCatalog.FreePlanId, tenant.PlanId);
    }

    [Fact]
    public async Task Bootstrap_RunTwice_DoesNotDuplicateInstallationTenant()
    {
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);
        await AdminBootstrapper.RunAsync(_factory.Services, CancellationToken.None);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var count = await context.Tenants.CountAsync(t => t.Id == BootstrapWebApplicationFactory.BootstrapTenantId);

        Assert.Equal(1, count);
    }
}
