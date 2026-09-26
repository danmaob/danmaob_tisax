using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Plans;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantModuleEndpointsTests : IClassFixture<PlanGateWebApplicationFactory>
{
    private readonly PlanGateWebApplicationFactory _factory;

    public TenantModuleEndpointsTests(PlanGateWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        return _factory.CreateClientWithToken(login.AccessToken);
    }

    private async Task<Guid> CreateTenantOnPlanAsync(HttpClient client, Guid planId)
    {
        var tenant = await _factory.CreateTenantAsync(client);
        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, planId);
        changeResponse.EnsureSuccessStatusCode();
        return tenant.Id;
    }

    private async Task<HttpStatusCode> GetEvidenceStatusAsTenantAsync(Guid tenantId)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTokenForTenant(tenantId));
        var response = await client.GetAsync("/api/v1/test-plan-probe/evidence");
        return response.StatusCode;
    }

    private static Task<HttpResponseMessage> SetExceptionAsync(HttpClient client, Guid tenantId, string moduleCode, string state)
    {
        return client.PutAsJsonAsync("/api/v1/platform/tenants/" + tenantId + "/modules/" + moduleCode, new { State = state });
    }

    [Fact]
    public async Task EnabledException_GivesAccessToOneTenant_WithoutAffectingAnotherTenantOnSamePlan()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        var tenantA = await CreateTenantOnPlanAsync(client, plan.Id);
        var tenantB = await CreateTenantOnPlanAsync(client, plan.Id);
        var response = await SetExceptionAsync(client, tenantA, FunctionalModuleCodes.Evidence, TenantModuleExceptionStates.Enabled);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, await GetEvidenceStatusAsTenantAsync(tenantA));
        Assert.Equal(HttpStatusCode.Forbidden, await GetEvidenceStatusAsTenantAsync(tenantB));
    }

    [Fact]
    public async Task InheritClearsException_ModuleFollowsPlanAgain()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client);
        var tenantId = await CreateTenantOnPlanAsync(client, plan.Id);
        var enableResponse = await SetExceptionAsync(client, tenantId, FunctionalModuleCodes.Evidence, TenantModuleExceptionStates.Enabled);
        Assert.Equal(HttpStatusCode.OK, enableResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, await GetEvidenceStatusAsTenantAsync(tenantId));
        var inheritResponse = await SetExceptionAsync(client, tenantId, FunctionalModuleCodes.Evidence, TenantModuleExceptionStates.Inherit);
        Assert.Equal(HttpStatusCode.OK, inheritResponse.StatusCode);
        var state = await inheritResponse.Content.ReadFromJsonAsync<TenantModuleStateDto>();
        Assert.NotNull(state);
        Assert.Null(state.ExceptionIsEnabled);
        Assert.False(state.IsEnabled);
        Assert.Equal(HttpStatusCode.Forbidden, await GetEvidenceStatusAsTenantAsync(tenantId));
    }

    [Fact]
    public async Task DisabledException_DeniesModuleThatThePlanIncludes()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        var tenantId = await CreateTenantOnPlanAsync(client, plan.Id);
        Assert.Equal(HttpStatusCode.OK, await GetEvidenceStatusAsTenantAsync(tenantId));
        var response = await SetExceptionAsync(client, tenantId, FunctionalModuleCodes.Evidence, TenantModuleExceptionStates.Disabled);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, await GetEvidenceStatusAsTenantAsync(tenantId));
    }
}
