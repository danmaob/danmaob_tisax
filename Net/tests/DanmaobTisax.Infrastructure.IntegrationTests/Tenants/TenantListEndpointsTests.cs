using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantListEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public TenantListEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string NewMarker() => Guid.NewGuid().ToString("N");

    private static async Task<PagedResult<TenantDto>> GetTenantsAsync(HttpClient client, string query)
    {
        var response = await client.GetAsync("/api/v1/platform/tenants?" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TenantDto>>();
        Assert.NotNull(page);
        return page;
    }

    [Fact]
    public async Task ListTenants_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/platform/tenants");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListTenants_WithTenantTokenHoldingManageTenantsPermission_ReturnsForbidden()
    {
        var (userId, token) = await _factory.CreateUserWithPlatformPermissionsAndLoginAsync(new[] { "ManageTenants" });
        var client = _factory.CreateClientWithToken(token);
        var response = await client.GetAsync("/api/v1/platform/tenants");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListTenants_WithoutFilters_ReturnsTenantsOrderedByName()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var marker = NewMarker();
        await _factory.CreateTenantAsync(client, marker + "-Bravo");
        await _factory.CreateTenantAsync(client, marker + "-Alpha");
        await _factory.CreateTenantAsync(client, marker + "-Charlie");
        var page = await GetTenantsAsync(client, "pageSize=200");
        var names = page.Items.Where(t => t.Name.StartsWith(marker)).Select(t => t.Name).ToList();
        Assert.Equal(new[] { marker + "-Alpha", marker + "-Bravo", marker + "-Charlie" }, names);
        Assert.True(page.TotalCount >= 3);
    }

    [Fact]
    public async Task ListTenants_WithPageSizeAndPageNumber_ReturnsRequestedPageAndTotalCount()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var marker = NewMarker();
        await _factory.CreateTenantAsync(client, marker + "-Alpha");
        await _factory.CreateTenantAsync(client, marker + "-Bravo");
        await _factory.CreateTenantAsync(client, marker + "-Charlie");
        var page = await GetTenantsAsync(client, "name=" + marker + "&pageSize=2&pageNumber=2");
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.PageNumber);
        Assert.Equal(2, page.PageSize);
        var item = Assert.Single(page.Items);
        Assert.Equal(marker + "-Charlie", item.Name);
    }

    [Fact]
    public async Task ListTenants_WithOutOfRangePaging_ClampsPageSizeAndPageNumber()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var page = await GetTenantsAsync(client, "pageSize=500&pageNumber=0");
        Assert.Equal(200, page.PageSize);
        Assert.Equal(1, page.PageNumber);
    }
}
