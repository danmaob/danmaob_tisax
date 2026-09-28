using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Infrastructure.IntegrationTests.Identity;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class InstallationTenantProtectionTests : IClassFixture<TenantStatusWebApplicationFactory>
{
    private readonly TenantStatusWebApplicationFactory _factory;

    public InstallationTenantProtectionTests(TenantStatusWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        return _factory.CreateClientWithToken(login.AccessToken);
    }

    [Fact]
    public async Task DeactivateInstallationTenant_ReturnsConflictWithErrorCode_AndTenantStaysActive()
    {
        await _factory.EnsureTenantRowAsync(AuthEndpointsWebApplicationFactory.TestTenantId);
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsync("/api/v1/platform/tenants/" + AuthEndpointsWebApplicationFactory.TestTenantId + "/deactivate", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Tenant.InstallationTenantCannotBeDeactivated", problem.GetProperty("errorCode").GetString());
        var tenant = await client.GetFromJsonAsync<TenantDto>("/api/v1/platform/tenants/" + AuthEndpointsWebApplicationFactory.TestTenantId);
        Assert.NotNull(tenant);
        Assert.Equal("Active", tenant.Status);
    }

    [Fact]
    public async Task SuspendAndReactivateInstallationTenant_AreStillAllowed()
    {
        await _factory.EnsureTenantRowAsync(AuthEndpointsWebApplicationFactory.TestTenantId);
        var client = await CreateAdminClientAsync();
        var suspendResponse = await client.PostAsync("/api/v1/platform/tenants/" + AuthEndpointsWebApplicationFactory.TestTenantId + "/suspend", null);
        Assert.Equal(HttpStatusCode.OK, suspendResponse.StatusCode);
        var reactivateResponse = await client.PostAsync("/api/v1/platform/tenants/" + AuthEndpointsWebApplicationFactory.TestTenantId + "/reactivate", null);
        Assert.Equal(HttpStatusCode.OK, reactivateResponse.StatusCode);
    }

    [Fact]
    public async Task DeactivateOtherTenant_IsStillAllowed()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);
        var response = await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
