using System.Net.Http.Json;
using DanmaobTisax.Application.Tenants;
namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class TenantCreateAndGetEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public TenantCreateAndGetEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string NewTenantName() => "Tenant-" + Guid.NewGuid();

    [Fact]
    public async Task CreateTenant_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var name = NewTenantName();

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithoutPermission_ReturnsForbidden()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(false);
        var client = _factory.CreateClientWithToken(token);
        var name = NewTenantName();

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithValidName_ReturnsCreatedWithActiveStatusAndLocation()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var name = NewTenantName();

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });
        var status = response.StatusCode;
        var locationText = response.Headers.Location?.ToString();

        Assert.Equal(System.Net.HttpStatusCode.Created, status);
        Assert.NotNull(locationText);

        var tenantDto = await response.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(tenantDto);
        Assert.Equal(name, tenantDto.Name);
        Assert.Equal("Active", tenantDto.Status);
        Assert.NotEqual(Guid.Empty, tenantDto.Id);
        Assert.EndsWith("/api/v1/platform/tenants/" + tenantDto.Id, locationText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateTenant_WithSurroundingSpaces_StoresTrimmedName()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var uniqueName = Guid.NewGuid().ToString();
        var nameWithSpaces = "  " + uniqueName + "  ";

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = nameWithSpaces });
        var status = response.StatusCode;

        Assert.Equal(System.Net.HttpStatusCode.Created, status);

        var tenantDto = await response.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(tenantDto);
        Assert.Equal(uniqueName, tenantDto.Name);
    }

    [Fact]
    public async Task CreateTenant_WithMissingName_ReturnsBadRequest()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new {});

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithWhitespaceName_ReturnsBadRequest()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var whitespaceName = "   ";

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = whitespaceName });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithNameLongerThan200_ReturnsBadRequest()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var longName = new string('a', 201);

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = longName });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithDuplicateName_ReturnsConflict()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var name = NewTenantName();

        var response1 = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });
        var status1 = response1.StatusCode;

        Assert.Equal(System.Net.HttpStatusCode.Created, status1);

        response1.Dispose();
        var response2 = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });
        var status2 = response2.StatusCode;

        Assert.Equal(System.Net.HttpStatusCode.Conflict, status2);
    }

    [Fact]
    public async Task CreateTenant_WithDuplicateNameDifferentCase_ReturnsConflict()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var name = NewTenantName();

        var response1 = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });
        var status1 = response1.StatusCode;

        Assert.Equal(System.Net.HttpStatusCode.Created, status1);

        response1.Dispose();
        var upperName = name.ToUpperInvariant();
        var response2 = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = upperName });
        var status2 = response2.StatusCode;

        Assert.Equal(System.Net.HttpStatusCode.Conflict, status2);
    }

    [Fact]
    public async Task GetTenantById_ExistingTenant_ReturnsTenant()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var name = NewTenantName();

        var createResponse = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var locationText = createResponse.Headers.Location?.ToString();

        createResponse.Dispose();

        Assert.NotNull(locationText);
        if (locationText is { } loc)
        {
            var idPart = loc.Split('/').LastOrDefault();
            Assert.NotNull(idPart);

            var getResponse = await client.GetAsync("/api/v1/platform/tenants/" + idPart);
            var status = getResponse.StatusCode;

            Assert.Equal(System.Net.HttpStatusCode.OK, status);

            var tenantDto = await getResponse.Content.ReadFromJsonAsync<TenantDto>();
            Assert.NotNull(tenantDto);
            Assert.NotEqual(Guid.Empty, tenantDto.Id);
            Assert.Equal(name, tenantDto.Name);
        }
    }

    [Fact]
    public async Task GetTenantById_UnknownId_ReturnsNotFound()
    {
        var (userId, token) = await _factory.CreateUserAndLoginAsync(true);
        var client = _factory.CreateClientWithToken(token);
        var unknownId = Guid.NewGuid();

        var response = await client.GetAsync("/api/v1/platform/tenants/" + unknownId);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTenantById_WithoutPermission_ReturnsForbidden()
    {
        var (authorizedUserId, authorizedToken) = await _factory.CreateUserAndLoginAsync(true);
        var authorizedClient = _factory.CreateClientWithToken(authorizedToken);

        var (noPermsUserId, noPermsToken) = await _factory.CreateUserAndLoginAsync(false);
        var noPermsClient = _factory.CreateClientWithToken(noPermsToken);

        var name = NewTenantName();
        var createResponse = await authorizedClient.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = name });
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var locationText = createResponse.Headers.Location?.ToString();

        createResponse.Dispose();
        Assert.NotNull(locationText);
        if (locationText is { } loc)
        {
            var idPart = loc.Split('/').LastOrDefault();
            Assert.NotNull(idPart);

            var getResponse = await noPermsClient.GetAsync("/api/v1/platform/tenants/" + idPart);
            var status = getResponse.StatusCode;

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, status);
        }
    }
}
