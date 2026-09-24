using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Plans;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantPlanEndpointsTests
{
    [Fact]
    public async Task ChangePlan_UnknownPlan_ReturnsBadRequestWithErrorCode()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);
        var response = await _factory.ChangeTenantPlanAsync(client, tenant.Id, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errorCode", out var errorCode));
        Assert.Equal(Tenant.PlanNotFound.ToString(), errorCode.GetString());

        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(fetched);
        Assert.Equal(PlanCatalog.FreePlanId, fetched.PlanId);
    }

    [Fact]
    public async Task ChangePlan_InactivePlan_ReturnsConflictWithErrorCode()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        
        var deactivateResponse = await client.PostAsync("/api/v1/platform/plans/" + plan.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var tenant = await _factory.CreateTenantAsync(client);
        var response = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errorCode", out var errorCode));
        Assert.Equal(Tenant.PlanInactive.ToString(), errorCode.GetString());
    }

    [Fact]
    public async Task DeactivatePlan_AssignedToActiveTenant_ReturnsConflictWithErrorCode()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        var tenant = await _factory.CreateTenantAsync(client);
        
        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        var response = await client.PostAsync("/api/v1/platform/plans/" + plan.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errorCode", out var errorCode));
        Assert.Equal(Plan.InUse.ToString(), errorCode.GetString());
    }

    [Fact]
    public async Task ChangePlan_IsRecordedInTenantAuditHistory()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);
        var plan = await _factory.CreatePlanAsync(client);
        
        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        var response = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id + "/audit");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var audit = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(audit);

        var updatedEntry = Assert.Single(audit, entry => entry.Action == "Updated");
        Assert.NotNull(updatedEntry.ChangedColumnsJson);
        Assert.Contains("PlanId", updatedEntry.ChangedColumnsJson);
    }
}
