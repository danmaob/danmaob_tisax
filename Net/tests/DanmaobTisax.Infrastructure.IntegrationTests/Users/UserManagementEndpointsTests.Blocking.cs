using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Users;

public partial class UserManagementEndpointsTests
{
    [Fact]
    public async Task DeactivatedUser_ExistingAccessToken_ReturnsUserInactive()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        var user = await CreateUserAsync(client, email, "Token Holder");
        var loginResponse = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(login);
        var userClient = _factory.CreateClientWithToken(login.AccessToken!);
        var beforeResponse = await userClient.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Forbidden, beforeResponse.StatusCode);
        var deactivateResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var afterResponse = await userClient.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Unauthorized, afterResponse.StatusCode);
        Assert.Equal("User.Inactive", await ReadErrorCodeAsync(afterResponse));
    }

    [Fact]
    public async Task DeactivatedUser_RefreshToken_IsRejected()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        var user = await CreateUserAsync(client, email, "Refresh Holder");
        var loginResponse = await LoginAsync(email);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(login);
        var deactivateResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var anonymousClient = _factory.CreateClient();
        var refreshResponse = await anonymousClient.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task PreexistingUserDeactivatedInDatabase_TokensAreRejected()
    {
        var email = NewEmail();
        using var seedScope = _factory.Services.CreateScope();
        var seedContext = seedScope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = seedScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User(UserManagementWebApplicationFactory.TestTenantId, email, passwordHasher.Hash(UserManagementWebApplicationFactory.ValidPassword), "Preexisting User");
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();
        var loginResponse = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(login);
        using var updateScope = _factory.Services.CreateScope();
        var updateContext = updateScope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var storedUser = await updateContext.Users.FirstAsync(u => u.Id == user.Id);
        storedUser.Deactivate();
        await updateContext.SaveChangesAsync();
        var userClient = _factory.CreateClientWithToken(login.AccessToken!);
        var response = await userClient.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("User.Inactive", await ReadErrorCodeAsync(response));
        var anonymousClient = _factory.CreateClient();
        var refreshResponse = await anonymousClient.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_RevokesActiveRefreshTokens()
    {
        var client = await CreateAdminClientAsync();
        var email = NewEmail();
        var user = await CreateUserAsync(client, email, "Token Revocation");
        var loginResponse = await LoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var deactivateResponse = await client.PostAsync("/api/v1/users/" + user.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var tokens = await context.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
        Assert.NotEmpty(tokens);
        Assert.All(tokens, t => Assert.NotNull(t.RevokedAtUtc));
    }
}
