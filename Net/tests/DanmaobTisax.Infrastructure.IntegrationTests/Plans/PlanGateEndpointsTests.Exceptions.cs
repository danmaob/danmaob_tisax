using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Domain.Plans;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanGateEndpointsTests
{
    [Fact]
    public async Task DisabledException_OverridesPlanThatIncludesModule_ReturnsForbidden()
    {
        var client = await CreateAdminClientAsync();
        var plan = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        var tenant = await _factory.CreateTenantAsync(client);

        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        var okResponse1 = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.OK, okResponse1);

        await _factory.SetTenantModuleAsync(tenant.Id, FunctionalModuleCodes.Evidence, false);

        var okResponse2 = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.Forbidden, okResponse2);
    }

    [Fact]
    public async Task EnabledException_OverridesPlanWithoutModule_ReturnsOk()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);

        var forbiddenResponse = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse);

        await _factory.SetTenantModuleAsync(tenant.Id, FunctionalModuleCodes.Evidence, true);

        var okResponse = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.OK, okResponse);
    }
}
