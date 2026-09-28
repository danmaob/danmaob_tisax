using System.Net;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public partial class PlatformAuditEndpointsTests
{
    [Fact]
    public async Task EntityFilter_Plan_ReturnsOnlyPlanEntries()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var plan = await _factory.CreatePlanAsync(client);
        var page = await GetAuditAsync(client, "entityName=Plan&pageSize=200");
        Assert.Contains(page.Items, e => e.EntityId == plan.Id.ToString());
        Assert.All(page.Items, e => Assert.Equal("Plan", e.EntityName));
    }

    [Fact]
    public async Task TenantScopedEntities_NeverAppearInPlatformAudit()
    {
        await _factory.CreateUserAndLoginAsync(false);
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var page = await GetAuditAsync(client, "entityName=User");
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task Export_ReturnsCsvWithHeaderAndTheTenantRow()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var tenant = await _factory.CreateTenantAsync(client);
        var response = await client.GetAsync("/api/v1/platform/audit/export?tenantId=" + tenant.Id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("PerformedAtUtc,Action,EntityName,EntityId,AffectedTenantId", csv);
        Assert.Contains(tenant.Id.ToString(), csv);
    }

    [Fact]
    public async Task TenantUserWithReadAuditPermission_IsForbidden()
    {
        var (userId, token) = await _factory.CreateUserWithPlatformPermissionsAndLoginAsync(new[] { "ReadAudit" });
        using var client = _factory.CreateClientWithToken(token);
        var queryResponse = await client.GetAsync("/api/v1/platform/audit");
        Assert.Equal(HttpStatusCode.Forbidden, queryResponse.StatusCode);
        var exportResponse = await client.GetAsync("/api/v1/platform/audit/export");
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);
    }
}
