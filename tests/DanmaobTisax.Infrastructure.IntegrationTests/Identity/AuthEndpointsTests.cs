using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public class AuthEndpointsTests : IClassFixture<AuthEndpointsWebApplicationFactory>
{
    private readonly AuthEndpointsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(AuthEndpointsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedTestUserAsync(string email, string plainTextPassword)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var user = new User(
            AuthEndpointsWebApplicationFactory.TestTenantId,
            email,
            passwordHasher.Hash(plainTextPassword),
            "Test User");

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedTestUserAsync(email, "Str0ng!Passw0rd");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Str0ng!Passw0rd" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(result);
        Assert.True(result!.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedTestUserAsync(email, "Str0ng!Passw0rd");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "WrongPassword1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Whatever1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedTestUserAsync(email, "Str0ng!Passw0rd");

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Str0ng!Passw0rd" });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new { RefreshToken = loginResult!.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(refreshResult);
        Assert.True(refreshResult!.Succeeded);
        Assert.NotEqual(loginResult.AccessToken, refreshResult.AccessToken);
        Assert.NotEqual(loginResult.RefreshToken, refreshResult.RefreshToken);
    }

    [Fact]
    public async Task Logout_ThenRefreshWithSameToken_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedTestUserAsync(email, "Str0ng!Passw0rd");

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Str0ng!Passw0rd" });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();

        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", new { RefreshToken = loginResult!.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new { RefreshToken = loginResult.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }
}
