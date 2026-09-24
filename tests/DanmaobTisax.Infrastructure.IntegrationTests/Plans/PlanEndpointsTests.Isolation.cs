using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanEndpointsTests
{
    [Fact]
    public async Task SetModules_EmptyList_DisablesAllModulesOfThePlan()
    {
        using var client = await CreatePlanAdminClientAsync();

        var plan = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });

        Assert.Contains("Evidence", plan.ModuleCodes);

        var response = await client.PutAsJsonAsync(
            $"api/v1/platform/plans/{plan.Id}/modules",
            new { ModuleCodes = Array.Empty<string>() });

        response.EnsureSuccessStatusCode();

        var planDto = await response.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(planDto);
        Assert.Empty(planDto.ModuleCodes);

        var fetchedPlan = await client.GetFromJsonAsync<PlanDto?>($"api/v1/platform/plans/{plan.Id}");
        Assert.NotNull(fetchedPlan);
        Assert.Empty(fetchedPlan.ModuleCodes);
    }

    [Fact]
    public async Task SetModules_OnOnePlan_DoesNotChangeAnotherPlan()
    {
        using var client = await CreatePlanAdminClientAsync();

        var planA = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });
        var planB = await _factory.CreatePlanAsync(client, new[] { FunctionalModuleCodes.Evidence });

        Assert.Contains("Evidence", planB.ModuleCodes);

        var responseB = await client.PutAsJsonAsync(
            $"api/v1/platform/plans/{planB.Id}/modules",
            new { ModuleCodes = Array.Empty<string>() });

        responseB.EnsureSuccessStatusCode();

        var planADto = await client.GetFromJsonAsync<PlanDto?>($"api/v1/platform/plans/{planA.Id}");
        Assert.Equal(new[] { "Evidence" }, planADto?.ModuleCodes);

        var planBDto = await client.GetFromJsonAsync<PlanDto?>($"api/v1/platform/plans/{planB.Id}");
        Assert.NotNull(planBDto);
        Assert.Empty(planBDto.ModuleCodes);
    }
}
