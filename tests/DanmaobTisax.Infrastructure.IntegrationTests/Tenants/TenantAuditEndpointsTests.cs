using System.Net;
using System.Text.Json;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class TenantAuditEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public TenantAuditEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<List<AuditLogDto>> GetAuditAsync(HttpClient client, Guid id)
    {
        var response = await client.GetAsync($"/api/v1/platform/tenants/{id}/audit");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<AuditLogDto>>(content, options) ?? [];
    }

    [Fact]
    public async Task AuditHistory_AfterCreateAndDeactivate_RecordsWhoAndWhen()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var before = DateTime.UtcNow;
        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/deactivate", null);

        var audit = await GetAuditAsync(client, tenant.Id);
        Assert.Equal(2, audit.Count);
        Assert.Equal("Created", audit[0].Action);
        Assert.Equal("Tenant", audit[0].EntityName);
        Assert.Equal(tenant.Id.ToString(), audit[0].EntityId);
        Assert.Equal(userId, audit[0].PerformedByUserId);
        Assert.Equal("Updated", audit[1].Action);
        Assert.Equal("Tenant", audit[1].EntityName);
        Assert.Equal(userId, audit[1].PerformedByUserId);
        var timestamp = audit[1].PerformedAtUtc;
        Assert.True(timestamp >= before);
        Assert.True(timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AuditHistory_StatusChange_RecordsOldAndNewValues()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/deactivate", null);

        var audit = await GetAuditAsync(client, tenant.Id);
        var updatedEntry = Assert.Single(audit, entry => entry.Action == "Updated");
        Assert.NotNull(updatedEntry.OldValuesJson);
        Assert.NotNull(updatedEntry.NewValuesJson);
        Assert.NotNull(updatedEntry.ChangedColumnsJson);
        Assert.Contains("Status", updatedEntry.ChangedColumnsJson);
        Assert.NotEqual(updatedEntry.OldValuesJson, updatedEntry.NewValuesJson);
    }

    [Fact]
    public async Task AuditHistory_AfterSuspendReactivateDeactivate_ListsEntriesInChronologicalOrder()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/suspend", null);
        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/reactivate", null);
        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/deactivate", null);

        var audit = await GetAuditAsync(client, tenant.Id);
        Assert.Equal(4, audit.Count);
        Assert.Equal("Created", audit[0].Action);
        Assert.Equal("Updated", audit[1].Action);
        Assert.Equal("Updated", audit[2].Action);
        Assert.Equal("Updated", audit[3].Action);
        var previous = DateTime.MinValue;
        foreach (var entry in audit)
        {
            Assert.True(entry.PerformedAtUtc >= previous);
            previous = entry.PerformedAtUtc;
        }
    }

    [Fact]
    public async Task AuditHistory_FailedTransition_DoesNotAddEntry()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/suspend", null);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/platform/tenants/{tenant.Id}/audit")).StatusCode);
        var secondResult = await client.PostAsync($"/api/v1/platform/tenants/{tenant.Id}/suspend", null);
        Assert.Equal(HttpStatusCode.Conflict, secondResult.StatusCode);

        var audit = await GetAuditAsync(client, tenant.Id);
        Assert.Equal(2, audit.Count);
    }

    [Fact]
    public async Task AuditHistory_ForTenantEvents_HasNullTenantId()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        await client.PostAsync("/api/v1/platform/tenants/" + tenant.Id + "/suspend", null);

        var audit = await GetAuditAsync(client, tenant.Id);
        Assert.All(audit, entry => Assert.Null(entry.TenantId));
    }

    [Fact]
    public async Task AuditHistory_UnknownTenant_ReturnsNotFound()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);

        var result = await client.GetAsync("/api/v1/platform/tenants/" + Guid.NewGuid() + "/audit");
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task AuditHistory_WithoutPermission_ReturnsForbidden()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var tenant = await _factory.CreateTenantAsync(client);

        var (userId2, token2) = await _factory.CreateUserAndLoginAsync(false);
        var client2 = _factory.CreateClientWithToken(token2);

        var result = await client2.GetAsync($"/api/v1/platform/tenants/{tenant.Id}/audit");
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
}
