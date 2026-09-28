using System.Net;
using System.Net.Http.Json;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public partial class PlatformAuthEndpointsTests
{
    [Fact]
    public async Task Login_AfterMaxFailedAttempts_IsLockedOutEvenWithCorrectPassword()
    {
        var email = Guid.NewGuid() + "@example.com";
        const string password = "Str0ng!Passw0rd";
        await SeedPlatformAdministratorAsync(email, password);
        using var client = _factory.CreateClient();
        for (int i = 0; i < 5; i++)
        {
            var failedResponse = await client.PostAsJsonAsync("/api/v1/platform/auth/login", new { Email = email, Password = "WrongPassw0rd!" });
            Assert.Equal(HttpStatusCode.Unauthorized, failedResponse.StatusCode);
        }
        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", new { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlatformAdministrator_IsNotStoredAsUserOfAnyTenant()
    {
        var email = Guid.NewGuid() + "@example.com";
        await SeedPlatformAdministratorAsync(email, "Str0ng!Passw0rd");
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var userCount = await context.Users.IgnoreQueryFilters().CountAsync(u => u.Email == email);
        var administratorCount = await context.PlatformAdministrators.CountAsync(a => a.Email == email);
        Assert.Equal(0, userCount);
        Assert.Equal(1, administratorCount);
    }

    [Fact]
    public async Task TenantUserWithPlatformPermission_IsForbiddenOnPlatformEndpoints()
    {
        var (userId, token) = await _factory.CreateUserWithPlatformPermissionsAndLoginAsync(new[] { "ManageTenants" });
        using var client = _factory.CreateClientWithToken(token);
        var response = await client.GetAsync("/api/v1/platform/tenants/" + Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
