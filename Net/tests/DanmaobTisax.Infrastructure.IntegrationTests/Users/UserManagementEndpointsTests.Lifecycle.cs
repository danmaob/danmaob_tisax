using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Users;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Users;

public partial class UserManagementEndpointsTests
{
    [Fact]
    public async Task DeactivateUser_WhenActive_BlocksLogin()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        var user = await CreateUserAsync(client, email, "To Deactivate");
        var firstLogin = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, firstLogin.StatusCode);
        var response = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(body);
        Assert.False(body.IsActive);
        var secondLogin = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.Unauthorized, secondLogin.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_Self_ReturnsConflictAndAdminStaysActive()
    {
        var (adminId, token) = await _factory.CreateUserWithIdentityPermissionsAndLoginAsync(new[] { "ManageUsers" });
        var client = _factory.CreateClientWithToken(token);
        var response = await client.PostAsync("/api/v1/users/" + adminId + "/deactivate", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("User.CannotDeactivateSelf", await ReadErrorCodeAsync(response));
        var listResponse = await client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_WhenAlreadyInactive_ReturnsConflict()
    {
        var client = await CreateAdminClientAsync();
        var user = await CreateUserAsync(client, NewEmail(), "Already Inactive");
        var firstResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var secondResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal("User.InvalidStatusTransition", await ReadErrorCodeAsync(secondResponse));
    }

    [Fact]
    public async Task ReactivateUser_AfterDeactivation_AllowsLoginAgain()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        var user = await CreateUserAsync(client, email, "To Reactivate");
        var deactivateResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var reactivateResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/reactivate", null);
        Assert.Equal(HttpStatusCode.OK, reactivateResponse.StatusCode);
        var loginResponse = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ListUsers_WithoutManageUsersPermission_ReturnsForbidden()
    {
        var (userId, token) = await _factory.CreateUserWithIdentityPermissionsAndLoginAsync(Array.Empty<string>());
        var client = _factory.CreateClientWithToken(token);
        var response = await client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListUsers_WithPlatformToken_ReturnsForbidden()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var response = await client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
