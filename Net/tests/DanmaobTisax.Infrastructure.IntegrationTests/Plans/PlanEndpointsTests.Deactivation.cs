using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanEndpointsTests
{
    [Fact]
    public async Task Deactivate_FreePlan_ReturnsConflictWithErrorCode()
    {
        using var client = await CreatePlanAdminClientAsync();

        // Try to deactivate the Free plan (seeded immutable plan)
        var response = await client.PostAsync(
            $"api/v1/platform/plans/{PlanCatalog.FreePlanId}/deactivate",
            null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var errorElement = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(errorElement.TryGetProperty("errorCode", out var errorCodeProperty));
        Assert.Equal("Plan.DefaultPlanCannotBeDeactivated", errorCodeProperty.GetString());
    }

    [Fact]
    public async Task DeactivateThenReactivate_UnusedPlan_TogglesIsActive()
    {
        using var client = await CreatePlanAdminClientAsync();

        // Create a new unused plan that can be toggled
        var plan = await _factory.CreatePlanAsync(client);

        // POST deactivate
        var response1 = await client.PostAsync(
            $"api/v1/platform/plans/{plan.Id}/deactivate",
            null);

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var planDto1 = await response1.Content.ReadFromJsonAsync<PlanDto>()!;
        Assert.True(planDto1!.IsActive == false);

        // POST deactivate again (should fail)
        var response2 = await client.PostAsync(
            $"api/v1/platform/plans/{plan.Id}/deactivate",
            null);

        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

        var errorElement = await response2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(errorElement.TryGetProperty("errorCode", out var errorCodeProperty));
        Assert.Equal("Plan.InvalidStateTransition", errorCodeProperty.GetString());

        // POST reactivate
        var response3 = await client.PostAsync(
            $"api/v1/platform/plans/{plan.Id}/reactivate",
            null);

        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var planDto2 = await response3.Content.ReadFromJsonAsync<PlanDto>()!;
        Assert.True(planDto2!.IsActive);
    }
}
