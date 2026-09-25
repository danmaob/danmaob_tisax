using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public partial class PlatformAuthEndpointsTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public PlatformAuthEndpointsTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedPlatformAdministratorAsync(string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        context.PlatformAdministrators.Add(new PlatformAdministrator(email, passwordHasher.Hash(password), "Platform Test Administrator"));
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessTokenWithoutTenantClaimAndWithoutRefreshToken()
    {
        var email = Guid.NewGuid() + "@example.com";
        const string password = "Str0ng!Passw0rd";
        await SeedPlatformAdministratorAsync(email, password);
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", new { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.AccessToken);
        Assert.Null(result.RefreshToken);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Null(token.Claims.FirstOrDefault(c => c.Type == "tenant"));
        Assert.Equal("platform", token.Claims.FirstOrDefault(c => c.Type == "principal_type")?.Value);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = Guid.NewGuid() + "@example.com";
        await SeedPlatformAdministratorAsync(email, "Str0ng!Passw0rd");
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", new { Email = email, Password = "WrongPassw0rd!" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithTenantUserCredentials_ReturnsUnauthorized()
    {
        var email = Guid.NewGuid() + "@example.com";
        const string password = "Str0ng!Passw0rd";
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            context.Users.Add(new User(AuthEndpointsWebApplicationFactory.TestTenantId, email, passwordHasher.Hash(password), "Tenant Test User"));
            await context.SaveChangesAsync();
        }
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", new { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
