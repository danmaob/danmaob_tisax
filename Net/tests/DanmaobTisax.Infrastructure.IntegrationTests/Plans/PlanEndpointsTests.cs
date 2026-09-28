using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public PlanEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreatePlanAdminClientAsync()
    {
        var (userId, token) = await _factory.CreatePlatformAdministratorAndLoginAsync();
        return _factory.CreateClientWithToken(token);
    }

    [Fact]
    public async Task ModuleCatalog_ReturnsTwentyModulesInCatalogOrder()
    {
        using var client = await CreatePlanAdminClientAsync();

        var response = await client.GetFromJsonAsync<List<FunctionalModuleDto>>("/api/v1/platform/plans/module-catalog");
        Assert.NotNull(response);

        List<FunctionalModuleDto> modules = response;
        Assert.Equal(20, modules.Count);
        Assert.Equal("Organization", modules[0].Code);
        Assert.Equal(1, modules[0].SortOrder);
        Assert.Equal("Administration", modules[19].Code);
        Assert.Equal(20, modules[19].SortOrder);
    }

    [Fact]
    public async Task GetAll_ContainsSeededFreeBasicAndPremiumPlans()
    {
        using var client = await CreatePlanAdminClientAsync();

        var response = await client.GetFromJsonAsync<List<PlanDto>>("/api/v1/platform/plans");
        Assert.NotNull(response);

        bool hasFree = response.Any(p => p.Code == PlanCatalog.FreeCode && p.Id == PlanCatalog.FreePlanId);
        bool hasBasic = response.Any(p => p.Code == PlanCatalog.BasicCode && p.Id == PlanCatalog.BasicPlanId);
        bool hasPremium = response.Any(p => p.Code == PlanCatalog.PremiumCode && p.Id == PlanCatalog.PremiumPlanId);

        Assert.True(hasFree, "Free plan should be in the catalog");
        Assert.True(hasBasic, "Basic plan should be in the catalog");
        Assert.True(hasPremium, "Premium plan should be in the catalog");
    }

    [Fact]
    public async Task GetAll_WithoutManagePlansPermission_ReturnsForbidden()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(false);
        using var client = _factory.CreateClientWithToken(token);

        var response = await client.GetAsync("/api/v1/platform/plans");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
