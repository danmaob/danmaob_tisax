using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Plans;
using DanmaobTisax.Domain;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

public partial class PlanEndpointsTests
{
    [Fact]
    public async Task SetModules_ValidCodes_ReplacesEnabledModules()
    {
        var permissions = new[] { "ManagePlans" };
        var result = await _factory.CreateUserWithPlatformPermissionsAndLoginAsync(permissions);
        using var adminClient = _factory.CreateClientWithToken(result.AccessToken);

        // First, create a plan with initial module codes
        var plan = await _factory.CreatePlanAsync(adminClient, new[] { FunctionalModuleCodes.Evidence });

        // PUT modules with two valid module codes (replacing all existing)
        var response1 = await adminClient.PutAsJsonAsync(
            $"api/v1/platform/plans/{plan.Id}/modules",
            new { ModuleCodes = new[] { FunctionalModuleCodes.Evidence, FunctionalModuleCodes.Risk } }
        );

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var planDto1 = await response1.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(planDto1);
        Assert.Equal(2, planDto1.ModuleCodes.Count);
        Assert.Contains("Evidence", planDto1.ModuleCodes);
        Assert.Contains("Risk", planDto1.ModuleCodes);

        // PUT modules again with only one valid module code (replaces all existing)
        var response2 = await adminClient.PutAsJsonAsync(
            $"api/v1/platform/plans/{plan.Id}/modules",
            new { ModuleCodes = new[] { FunctionalModuleCodes.Risk } }
        );

        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var planDto2 = await response2.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(planDto2);
        Assert.Single(planDto2.ModuleCodes);
        Assert.Equal("Risk", planDto2.ModuleCodes[0]);

        // GET the plan and verify it has only Risk module
        var getResponse = await adminClient.GetAsync($"api/v1/platform/plans/{plan.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedPlanDto = await getResponse.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(fetchedPlanDto);
        Assert.Single(fetchedPlanDto.ModuleCodes);
        Assert.Equal("Risk", fetchedPlanDto.ModuleCodes[0]);
    }

    [Fact]
    public async Task SetModules_UnknownModuleCode_ReturnsBadRequestWithErrorCode()
    {
        // Module codes must be valid functional module codes (e.g., from FunctionalModuleCodes)
        // Passing an invalid code like "InvalidModule" should return 400 BadRequest
        var permissions = new[] { "ManagePlans" };
        var result = await _factory.CreateUserWithPlatformPermissionsAndLoginAsync(permissions);
        using var adminClient = _factory.CreateClientWithToken(result.AccessToken);

        var plan = await _factory.CreatePlanAsync(adminClient, new[] { FunctionalModuleCodes.Evidence });

        // PUT with an unknown module code "NonExistentCode" that does not exist in FunctionalModuleCodes
        var response = await adminClient.PutAsJsonAsync(
            $"api/v1/platform/plans/{plan.Id}/modules",
            new { ModuleCodes = new[] { "NonExistentCode" } }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorElement = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(errorElement.TryGetProperty("errorCode", out var errorCodeProperty));
        Assert.NotNull(errorCodeProperty);
        Assert.Contains(PlanOperationOutcome.UnknownModuleCode.ToString(), errorCodeProperty.GetString());

        // GET the plan and verify that no modules were saved
        var getResponse = await adminClient.GetAsync($"api/v1/platform/plans/{plan.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedPlanDto = await getResponse.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(fetchedPlanDto);
        Assert.Empty(fetchedPlanDto.ModuleCodes);
    }
}
