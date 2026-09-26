using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Plans;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantModuleEndpointsTests
{
    [Fact]
    public async Task GetModules_ReturnsPlanExceptionAndEffectiveStatePerModule()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        var tenantId = await CreateTenantOnPlanAsync(client, plan.Id);
        var setResponse = await SetExceptionAsync(client, tenantId, FunctionalModuleCodes.Risk, TenantModuleExceptionStates.Enabled);
        Assert.Equal(HttpStatusCode.OK, setResponse.StatusCode);
        var response = await client.GetAsync("/api/v1/platform/tenants/" + tenantId + "/modules");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modules = await response.Content.ReadFromJsonAsync<List<TenantModuleStateDto>>();
        Assert.NotNull(modules);
        var evidence = modules.Single(m => m.ModuleCode == FunctionalModuleCodes.Evidence);
        Assert.True(evidence.EnabledByPlan);
        Assert.Null(evidence.ExceptionIsEnabled);
        Assert.True(evidence.IsEnabled);
        var risk = modules.Single(m => m.ModuleCode == FunctionalModuleCodes.Risk);
        Assert.False(risk.EnabledByPlan);
        Assert.True(risk.ExceptionIsEnabled);
        Assert.True(risk.IsEnabled);
    }

    [Fact]
    public async Task SetException_WithUnknownModuleCode_ReturnsBadRequestWithErrorCode()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        var tenantId = await CreateTenantOnPlanAsync(client, plan.Id);
        var response = await SetExceptionAsync(client, tenantId, "NotAModule", TenantModuleExceptionStates.Enabled);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Tenant.UnknownModuleCode", problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task SetException_WithInvalidState_ReturnsBadRequestWithErrorCode()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        var tenantId = await CreateTenantOnPlanAsync(client, plan.Id);
        var response = await SetExceptionAsync(client, tenantId, FunctionalModuleCodes.Evidence, "Maybe");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Tenant.InvalidModuleExceptionState", problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task UnknownTenant_ReturnsNotFoundOnGetAndSet()
    {
        var client = await CreateAdminClientAsync();
        var unknownTenantId = Guid.NewGuid();
        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + unknownTenantId + "/modules");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var setResponse = await SetExceptionAsync(client, unknownTenantId, FunctionalModuleCodes.Evidence, TenantModuleExceptionStates.Enabled);
        Assert.Equal(HttpStatusCode.NotFound, setResponse.StatusCode);
    }
}
