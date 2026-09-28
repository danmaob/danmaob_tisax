namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

using System.Net;
using System.Text.Json;
using DanmaobTisax.Application.Tenants;
using System.Net.Http.Json;

public class TenantLifecycleEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public TenantLifecycleEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string ActionUrl(Guid id, string action)
    {
        return "/api/v1/platform/tenants/" + id + "/" + action;
    }

    [Fact]
    public async Task Suspend_ActiveTenant_ReturnsSuspended()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var suspendResponse = await client.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        var suspendBody = await suspendResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(suspendBody);
        Assert.Equal("Suspended", suspendBody.Status);

        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        var getStatus = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(getStatus);
        Assert.Equal("Suspended", getStatus.Status);
    }

    [Fact]
    public async Task Suspend_AlreadySuspendedTenant_ReturnsConflictWithErrorCode()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var firstResponse = await client.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(firstBody);
        Assert.Equal("Suspended", firstBody.Status);

        var secondResponse = await client.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        var secondBodyElement = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.True(secondBodyElement.TryGetProperty("errorCode", out var errorCodeProperty));
        Assert.Equal("Tenant.InvalidStatusTransition", errorCodeProperty.GetString());
    }

    [Fact]
    public async Task Reactivate_SuspendedTenant_ReturnsActive()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var suspendResponse = await client.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        var suspendBody = await suspendResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(suspendBody);
        Assert.Equal("Suspended", suspendBody.Status);

        var reactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "reactivate"), null);
        var reactivateBody = await reactivateResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(reactivateBody);
        Assert.Equal("Active", reactivateBody.Status);
    }

    [Fact]
    public async Task Reactivate_ActiveTenant_ReturnsConflict()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var reactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "reactivate"), null);
        Assert.Equal(HttpStatusCode.Conflict, reactivateResponse.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ActiveTenant_ReturnsDeactivatedAndKeepsTenantRetrievable()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var deactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "deactivate"), null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var deactivateBody = await deactivateResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(deactivateBody);
        Assert.Equal("Deactivated", deactivateBody.Status);

        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        var getStatus = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(getStatus);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal("Deactivated", getStatus.Status);
        Assert.Equal(tenant.Name, getStatus.Name);
    }

    [Fact]
    public async Task Deactivate_SuspendedTenant_ReturnsDeactivated()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var suspendResponse = await client.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        var suspendBody = await suspendResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(suspendBody);
        Assert.Equal("Suspended", suspendBody.Status);

        var deactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "deactivate"), null);
        var deactivateBody = await deactivateResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(deactivateBody);
        Assert.Equal("Deactivated", deactivateBody.Status);
    }

    [Fact]
    public async Task Deactivate_DeactivatedTenant_ReturnsConflict()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var deactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "deactivate"), null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var secondDeactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "deactivate"), null);
        Assert.Equal(HttpStatusCode.Conflict, secondDeactivateResponse.StatusCode);
    }

    [Fact]
    public async Task Reactivate_DeactivatedTenant_ReturnsConflict()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var deactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "deactivate"), null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var reactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "reactivate"), null);
        Assert.Equal(HttpStatusCode.Conflict, reactivateResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        var getStatus = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(getStatus);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal("Deactivated", getStatus.Status);
    }

    [Fact]
    public async Task Suspend_DeactivatedTenant_ReturnsConflict()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var deactivateResponse = await client.PostAsync(ActionUrl(tenant.Id, "deactivate"), null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var suspendResponse = await client.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        Assert.Equal(HttpStatusCode.Conflict, suspendResponse.StatusCode);
    }

    [Fact]
    public async Task Suspend_UnknownTenant_ReturnsNotFound()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var unknownTenantId = Guid.NewGuid();

        var response = await client.PostAsync(ActionUrl(unknownTenantId, "suspend"), null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_UnknownTenant_ReturnsNotFound()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var unknownTenantId = Guid.NewGuid();

        var response = await client.PostAsync(ActionUrl(unknownTenantId, "reactivate"), null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_UnknownTenant_ReturnsNotFound()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var unknownTenantId = Guid.NewGuid();

        var response = await client.PostAsync(ActionUrl(unknownTenantId, "deactivate"), null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Suspend_WithoutPermission_ReturnsForbiddenAndLeavesTenantActive()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var (userId2, token2) = await _factory.CreateUserAndLoginAsync(false);
        var client2 = _factory.CreateClientWithToken(token2);

        var response = await client2.PostAsync(ActionUrl(tenant.Id, "suspend"), null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        var getStatus = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(getStatus);
        Assert.Equal("Active", getStatus.Status);
    }
}
