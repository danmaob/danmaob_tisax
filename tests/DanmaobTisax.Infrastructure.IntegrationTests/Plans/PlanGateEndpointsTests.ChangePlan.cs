using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Plans;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanGateEndpointsTests
{
    [Fact]
    public async Task Upgrade_ToPlanWithModule_GrantsAccessImmediately()
    {
        var client = await CreateAdminClientAsync();
        var tenant = await _factory.CreateTenantAsync(client);
        
        var evidenceStatusResponseBefore = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.Forbidden, evidenceStatusResponseBefore);
        
        var plan = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        
        var changeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, plan.Id);
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);
        
        var evidenceStatusResponseAfter = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.OK, evidenceStatusResponseAfter);
    }

    [Fact]
    public async Task Downgrade_ToPlanWithoutModule_BlocksAccessAndKeepsTenant()
    {
        var client = await CreateAdminClientAsync();
        
        var planWith = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        var planWithout = await _factory.CreatePlanAsync(client);
        
        var tenant = await _factory.CreateTenantAsync(client);
        
        var upgradeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, planWith.Id);
        Assert.Equal(HttpStatusCode.OK, upgradeResponse.StatusCode);
        
        var evidenceStatusAfterUpgrade = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.OK, evidenceStatusAfterUpgrade);
        
        var downgradeResponse = await _factory.ChangeTenantPlanAsync(client, tenant.Id, planWithout.Id);
        Assert.Equal(HttpStatusCode.OK, downgradeResponse.StatusCode);
        
        var evidenceStatusAfterDowngrade = await GetEvidenceStatusAsTenantAsync(tenant.Id);
        Assert.Equal(HttpStatusCode.Forbidden, evidenceStatusAfterDowngrade);
        
        var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + tenant.Id);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var fetched = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(fetched);
        
        Assert.Equal(tenant.Id, fetched.Id);
        Assert.Equal(tenant.Name, fetched.Name);
        Assert.Equal("Active", fetched.Status);
        Assert.Equal(planWithout.Id, fetched.PlanId);
    }
}
