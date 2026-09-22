// Copyright DANMAOB - All Rights Reserved
// This code is proprietary and confidential.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantStatusEndpointsTests : IClassFixture<TenantStatusWebApplicationFactory>
{
    private readonly TenantStatusWebApplicationFactory _factory;

    public TenantStatusEndpointsTests(TenantStatusWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient ClientFor(Guid tenantId)
    {
        return _factory.CreateClientWithToken(_factory.CreateTokenForTenant(tenantId));
    }

    [Fact]
    public async Task Open_TenantWithoutRow_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var client = ClientFor(tenantId);

        var response = await client.GetAsync("/api/v1/test-probe-status/open");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Open_ActiveTenantRow_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();

        await _factory.EnsureTenantRowAsync(tenantId);

        var client = ClientFor(tenantId);

        var response = await client.GetAsync("/api/v1/test-probe-status/open");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Open_SuspendedTenant_ReturnsForbiddenWithErrorCode()
    {
        var tenantId = Guid.NewGuid();

        await _factory.EnsureTenantRowAsync(tenantId);
        await _factory.SuspendTenantRowAsync(tenantId);

        var client = ClientFor(tenantId);

        var response = await client.GetAsync("/api/v1/test-probe-status/open");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var value = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(value);

        var errorCode = value.GetProperty("errorCode").GetString();
        Assert.Equal("Tenant.NotActive", errorCode);
    }
}
