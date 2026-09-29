using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Users;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Users;

public partial class UserManagementEndpointsTests : IClassFixture<UserManagementWebApplicationFactory>
{
    private readonly UserManagementWebApplicationFactory _factory;

    public UserManagementEndpointsTests(UserManagementWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string NewEmail() => Guid.NewGuid().ToString("N") + "@example.com";

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var (adminId, token) = await _factory.CreateUserWithIdentityPermissionsAndLoginAsync(new[] { "ManageUsers" });
        return _factory.CreateClientWithToken(token);
    }

    private static async Task<UserDto> CreateUserAsync(HttpClient client, string email, string fullName)
    {
        var response = await client.PostAsJsonAsync("/api/v1/users", new { Email = email, FullName = fullName, Password = UserManagementWebApplicationFactory.ValidPassword });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        return user;
    }

    private async Task<HttpResponseMessage> LoginAsync(string email)
    {
        var client = _factory.CreateClient();
        return await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = UserManagementWebApplicationFactory.ValidPassword });
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        return problem.GetProperty("errorCode").GetString();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsActiveUserThatCanLogIn()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        var user = await CreateUserAsync(client, email, "Created User");
        Assert.True(user.IsActive);
        Assert.Equal(email, user.Email);
        var loginResponse = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithSameEmailInDifferentCase_ReturnsConflict()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        await CreateUserAsync(client, email, "First User");
        var response = await client.PostAsJsonAsync("/api/v1/users", new { Email = email.ToUpperInvariant(), FullName = "Second User", Password = UserManagementWebApplicationFactory.ValidPassword });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("User.EmailAlreadyExists", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task CreateUser_WithWeakPassword_ReturnsBadRequest()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/v1/users", new { Email = NewEmail(), FullName = "Weak Password User", Password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User.PasswordPolicyViolation", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task ListUsers_FilteredBySearchAndStatus_ReturnsOnlyMatchingUser()
    {
        var client = await CreateAdminClientAsync();
        var marker = Guid.NewGuid().ToString("N");
        await CreateUserAsync(client, NewEmail(), marker + " Active");
        var inactive = await CreateUserAsync(client, NewEmail(), marker + " Inactive");
        var deactivateResponse = await client.PostAsync("/api/v1/users/" + inactive.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var response = await client.GetAsync("/api/v1/users?isActive=false&search=" + marker);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
        Assert.NotNull(page);
        var item = Assert.Single(page.Items);
        Assert.Equal(inactive.Id, item.Id);
    }

    [Fact]
    public async Task RenameUser_WithValidName_ReturnsUpdatedUser()
    {
        var client = await CreateAdminClientAsync();
        var user = await CreateUserAsync(client, NewEmail(), "Old Name");
        var response = await client.PutAsJsonAsync("/api/v1/users/" + user.Id + "/name", new { FullName = "New Name" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var getResponse = await client.GetAsync("/api/v1/users/" + user.Id);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var loaded = await getResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(loaded);
        Assert.Equal("New Name", loaded.FullName);
    }

    [Fact]
    public async Task GetUserById_WithUnknownId_ReturnsNotFound()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.GetAsync("/api/v1/users/" + Guid.NewGuid());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
