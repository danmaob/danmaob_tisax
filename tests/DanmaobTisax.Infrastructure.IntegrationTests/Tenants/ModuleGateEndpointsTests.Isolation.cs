using System.Net;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class ModuleGateEndpointsTests
{
    [Fact]
    public async Task Gated_ModuleEnabledOnlyForOtherTenant_ReturnsForbidden()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await _factory.SetModuleEnabledAsync(tenantA, ModuleGateProbeController.GatedModuleCode, true);
        await _factory.SetModuleEnabledAsync(tenantB, ModuleGateProbeController.GatedModuleCode, false);

        using var clientA = ClientFor(tenantA);
        using var clientB = ClientFor(tenantB);

        var responseA = await clientA.GetAsync("/api/v1/test-probe/gated");
        Assert.NotNull(responseA.Content);
        Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);

        var responseB = await clientB.GetAsync("/api/v1/test-probe/gated");
        Assert.NotNull(responseB.Content);
        Assert.Equal(HttpStatusCode.Forbidden, responseB.StatusCode);
    }

    [Fact]
    public async Task Gated_DifferentModuleEnabled_ReturnsForbidden()
    {
        var tenantId = Guid.NewGuid();

        await _factory.SetModuleEnabledAsync(tenantId, "Other.Module", true);

        using var client = ClientFor(tenantId);
        var response = await client.GetAsync("/api/v1/test-probe/gated");

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
