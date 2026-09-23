using System.Net;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantStatusEndpointsTests
{
    [Fact]
    public async Task Open_ReactivatedTenant_ReturnsOkImmediately()
    {
        var tenantId = Guid.NewGuid();
        await _factory.EnsureTenantRowAsync(tenantId);
        await _factory.SuspendTenantRowAsync(tenantId);

        using var clientWithToken = ClientFor(tenantId);

        var getResponseFirst = await clientWithToken.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.Forbidden, getResponseFirst.StatusCode);

        await _factory.ReactivateTenantRowAsync(tenantId);

        var getResponseSecond = await clientWithToken.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.OK, getResponseSecond.StatusCode);
    }

    [Fact]
    public async Task Open_DeactivatedTenant_ReturnsForbidden()
    {
        var tenantId = Guid.NewGuid();
        await _factory.EnsureTenantRowAsync(tenantId);
        await _factory.DeactivateTenantRowAsync(tenantId);

        using var clientWithToken = ClientFor(tenantId);

        var response = await clientWithToken.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Open_AnonymousRequest_ReturnsOk()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
