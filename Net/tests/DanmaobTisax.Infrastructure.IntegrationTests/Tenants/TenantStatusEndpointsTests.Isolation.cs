using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantStatusEndpointsTests
{
    [Fact]
    public async Task Open_SuspendedTenantDoesNotAffectOtherTenant()
    {
        var tenantIdA = Guid.NewGuid();
        await _factory.SuspendTenantRowAsync(tenantIdA);

        using var clientA = ClientFor(tenantIdA);
        var responseA = await clientA.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.Forbidden, responseA.StatusCode);

        var tenantIdB = Guid.NewGuid();
        await _factory.EnsureTenantRowAsync(tenantIdB);

        using var clientB = ClientFor(tenantIdB);
        var responseB = await clientB.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);
    }

    [Fact]
    public async Task Open_TenantSuspendedThroughPlatformApi_ReturnsForbiddenUntilReactivated()
    {
        var (userId, adminToken) = await _factory.CreateUserAndLoginAsync(true);
        using var adminClient = _factory.CreateClientWithToken(adminToken);

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/v1/platform/tenants",
            new { Name = "Tenant-" + Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var tenantDto = await createResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(tenantDto);

        var suspendResponse = await adminClient.PostAsync(
            $"/api/v1/platform/tenants/{tenantDto.Id}/suspend",
            null);
        Assert.Equal(HttpStatusCode.OK, suspendResponse.StatusCode);

        using var tenantClient = ClientFor(tenantDto.Id);
        var getResponse = await tenantClient.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);

        var reactivateResponse = await adminClient.PostAsync(
            $"/api/v1/platform/tenants/{tenantDto.Id}/reactivate",
            null);
        Assert.Equal(HttpStatusCode.OK, reactivateResponse.StatusCode);

        getResponse = await tenantClient.GetAsync("/api/v1/test-probe-status/open");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
