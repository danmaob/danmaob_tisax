using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantPlanEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public TenantPlanEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var login = await _factory.CreateUserWithPlatformPermissionsAndLoginAsync(new[] { "ManageTenants", "ManagePlans" });
        return _factory.CreateClientWithToken(login.AccessToken);
    }

    [Fact]
    public async Task CreateTenant_AssignsFreePlanAutomatically()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);
        Assert.Equal(PlanCatalog.FreePlanId, tenant.PlanId);

        var response = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fetched = await response.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(fetched);
        Assert.Equal(PlanCatalog.FreePlanId, fetched.PlanId);
    }

    [Fact]
    public async Task ChangePlan_ToActivePlan_ReturnsTenantWithNewPlan()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        var tenant = await _factory.CreateTenantAsync(client);
        var response = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var changed = await response.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(changed);
        Assert.Equal(plan.Id, changed.PlanId);

        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(fetched);
        Assert.Equal(plan.Id, fetched.PlanId);
    }

    [Fact]
    public async Task ChangePlan_UnknownTenant_ReturnsNotFound()
    {
        var client = await CreateAdminClientAsync();
        var response = await _factory.ChangeTenantPlanAsync(client, Guid.NewGuid(), PlanCatalog.BasicPlanId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
