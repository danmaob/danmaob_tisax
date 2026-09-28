using System.Net;
using System.Net.Http.Headers;
namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class ModuleGateEndpointsTests
{
    [Fact]
    public async Task Gated_ModuleExplicitlyDisabled_ReturnsForbidden()
    {
        var tenantId = Guid.NewGuid();
        await _factory.SetModuleEnabledAsync(tenantId, ModuleGateProbeController.GatedModuleCode, false);

        var client = ClientFor(tenantId);
        var response = await client.GetAsync("/api/v1/test-probe/gated");

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Gated_DisableAfterEnable_BlocksImmediatelyAndReEnableRestoresAccess()
    {
        var tenantId = Guid.NewGuid();
        await _factory.SetModuleEnabledAsync(tenantId, ModuleGateProbeController.GatedModuleCode, true);

        using var client = ClientFor(tenantId);

        var response1 = await client.GetAsync("/api/v1/test-probe/gated");
        Assert.NotNull(response1.Content);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        await _factory.SetModuleEnabledAsync(tenantId, ModuleGateProbeController.GatedModuleCode, false);

        var response2 = await client.GetAsync("/api/v1/test-probe/gated");
        Assert.NotNull(response2.Content);
        Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);

        await _factory.SetModuleEnabledAsync(tenantId, ModuleGateProbeController.GatedModuleCode, true);

        var response3 = await client.GetAsync("/api/v1/test-probe/gated");
        Assert.NotNull(response3.Content);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }
}
