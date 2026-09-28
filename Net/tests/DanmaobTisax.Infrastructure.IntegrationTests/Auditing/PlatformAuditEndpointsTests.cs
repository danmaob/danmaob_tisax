using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public partial class PlatformAuditEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public PlatformAuditEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<PagedResult<PlatformAuditLogDto>> GetAuditAsync(HttpClient client, string query)
    {
        var response = await client.GetAsync("/api/v1/platform/audit?" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<PlatformAuditLogDto>>();
        Assert.NotNull(page);
        return page;
    }

    [Fact]
    public async Task TenantCreation_AppearsWithActionWhoWhenAndAffectedTenant()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var tenant = await _factory.CreateTenantAsync(client);
        var page = await GetAuditAsync(client, "tenantId=" + tenant.Id + "&entityName=Tenant&action=Created");
        var entry = Assert.Single(page.Items);
        Assert.Equal("Created", entry.Action);
        Assert.Equal(tenant.Id, entry.AffectedTenantId);
        Assert.Equal(login.UserId, entry.PerformedByUserId);
        Assert.NotNull(entry.PerformedByDisplayName);
        Assert.EndsWith("@example.com", entry.PerformedByDisplayName);
        Assert.True(entry.PerformedAtUtc > DateTime.MinValue);
    }

    [Fact]
    public async Task PlanChange_AppearsAsTenantUpdateWithPlanIdColumn()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var tenant = await _factory.CreateTenantAsync(client);
        var plan = await _factory.CreatePlanAsync(client);
        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);
        changeResponse.EnsureSuccessStatusCode();
        var page = await GetAuditAsync(client, "tenantId=" + tenant.Id + "&action=Updated");
        var entry = Assert.Single(page.Items);
        Assert.Equal("Tenant", entry.EntityName);
        Assert.NotNull(entry.ChangedColumnsJson);
        Assert.Contains("PlanId", entry.ChangedColumnsJson);
    }

    [Fact]
    public async Task ModuleException_AppearsWithAffectedTenant()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var tenant = await _factory.CreateTenantAsync(client);
        var putResponse = await client.PutAsJsonAsync("/api/v1/platform/tenants/" + tenant.Id + "/modules/" + FunctionalModuleCodes.Evidence, new { State = TenantModuleExceptionStates.Enabled });
        putResponse.EnsureSuccessStatusCode();
        var page = await GetAuditAsync(client, "tenantId=" + tenant.Id + "&entityName=TenantModule");
        var entry = Assert.Single(page.Items);
        Assert.Equal("Created", entry.Action);
        Assert.Equal(tenant.Id, entry.AffectedTenantId);
    }
}
