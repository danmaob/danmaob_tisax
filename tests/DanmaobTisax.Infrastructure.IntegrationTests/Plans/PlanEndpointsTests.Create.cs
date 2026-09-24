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
    public async Task Create_ValidPlan_ReturnsCreatedWithUppercaseCodeAndNoModules()
    {
        var (_, accessToken) = await _factory.CreateUserWithPlanManagementAndLoginAsync();
        using var client = _factory.CreateClientWithToken(accessToken);

        var code = "t" + Guid.NewGuid().ToString("N");
        var response = await client.PostAsJsonAsync(
            "/api/v1/platform/plans",
            new { Code = code, Name = "Created plan" }
        );

        Assert.True(response.IsSuccessStatusCode);
        var planDto = await response.Content.ReadFromJsonAsync<PlanDto>();

        Assert.NotNull(planDto);
        Assert.Equal(code.ToUpperInvariant(), planDto.Code);
        Assert.Equal("Created plan", planDto.Name);
        Assert.True(planDto.IsActive);
        Assert.Empty(planDto.ModuleCodes);
    }

    [Fact]
    public async Task Create_DuplicateCode_ReturnsConflictWithErrorCode()
    {
        var (_, accessToken) = await _factory.CreateUserWithPlanManagementAndLoginAsync();
        using var client = _factory.CreateClientWithToken(accessToken);

        // First creation should succeed (this creates plan "T892F7A8C1")
        var existing = await _factory.CreatePlanAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/platform/plans",
            new { Code = existing.Code.ToLowerInvariant(), Name = "Duplicate" }
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var errorElement = await response.Content.ReadFromJsonAsync<JsonElement>();

        var errorCodeProperty = errorElement.GetProperty("errorCode");
        Assert.Equal("Plan.CodeAlreadyExists", errorCodeProperty.GetString());
    }

    [Fact]
    public async Task Rename_ValidName_ReturnsRenamedPlan()
    {
        var (_, accessToken) = await _factory.CreateUserWithPlanManagementAndLoginAsync();
        using var client = _factory.CreateClientWithToken(accessToken);

        var plan = await _factory.CreatePlanAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/platform/plans/{plan.Id}/name",
            new { Name = "Renamed plan" }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var renamedResponse = await response.Content.ReadFromJsonAsync<PlanDto>();

        Assert.NotNull(renamedResponse);
        Assert.Equal("Renamed plan", renamedResponse.Name);

        var getResponse = await client.GetFromJsonAsync<PlanDto>($"/api/v1/platform/plans/{plan.Id}");

        Assert.NotNull(getResponse);
        Assert.Equal("Renamed plan", getResponse.Name);
    }
}
