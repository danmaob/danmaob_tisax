using System.Net;
using System.Net.Http.Headers;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class ModuleGateEndpointsTests : IClassFixture<ModuleGateWebApplicationFactory>
{
    private readonly ModuleGateWebApplicationFactory _factory;

    public ModuleGateEndpointsTests(ModuleGateWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient ClientFor(Guid tenantId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTokenForTenant(tenantId));
        return client;
    }

    [Fact]
    public async Task Gated_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/test-probe/gated");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Gated_TenantWithoutEntitlementRow_ReturnsForbidden()
    {
        var tenantId = Guid.NewGuid();
        var client = ClientFor(tenantId);
        var response = await client.GetAsync("/api/v1/test-probe/gated");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Gated_ModuleEnabled_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        await _factory.SetModuleEnabledAsync(tenantId, "Probe.Module", true);

        using var client = ClientFor(tenantId);
        var response = await client.GetAsync("/api/v1/test-probe/gated");

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
