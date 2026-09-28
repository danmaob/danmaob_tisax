using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Plans;

using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanGateEndpointsTests : IClassFixture<PlanGateWebApplicationFactory>
{
    private readonly PlanGateWebApplicationFactory _factory;

    public PlanGateEndpointsTests(PlanGateWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        return _factory.CreateClientWithToken(login.AccessToken);
    }

    private async Task<HttpStatusCode> GetEvidenceStatusAsTenantAsync(Guid tenantId)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTokenForTenant(tenantId));
        var response = await client.GetAsync("/api/v1/test-plan-probe/evidence");
        return response.StatusCode;
    }

    [Fact]
    public async Task NewTenant_OnFreePlanWithoutModule_ReturnsForbidden()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);

        var status = await GetEvidenceStatusAsTenantAsync(tenant.Id);

        Assert.Equal(HttpStatusCode.Forbidden, status);
    }

    [Fact]
    public async Task TenantOnPlanThatIncludesModule_ReturnsOk()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        var tenant = await _factory.CreateTenantAsync(client);

        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        var status = await GetEvidenceStatusAsTenantAsync(tenant.Id);

        Assert.Equal(HttpStatusCode.OK, status);
    }
}
