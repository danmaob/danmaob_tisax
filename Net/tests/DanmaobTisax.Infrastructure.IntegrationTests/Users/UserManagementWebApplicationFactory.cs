using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Users;

public class UserManagementWebApplicationFactory : PlatformAdminWebApplicationFactory
{
    public const string ValidPassword = "Str0ng!Passw0rd";

    public async Task<(Guid UserId, string AccessToken)> CreateUserWithIdentityPermissionsAndLoginAsync(IReadOnlyList<string> identityActions)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = Guid.NewGuid() + "@example.com";

        var user = new User(TestTenantId, email, passwordHasher.Hash(ValidPassword), "Identity Test User");
        var role = new Role(TestTenantId, "IdentityTestRole-" + Guid.NewGuid(), null, false);
        var userRole = new UserRole(user.Id, role.Id);

        context.Users.Add(user);
        context.Roles.Add(role);
        context.UserRoles.Add(userRole);

        await context.SaveChangesAsync();

        foreach (var action in identityActions)
        {
            var permission = await context.Permissions.FirstAsync(p => p.Module == "Identity" && p.Action == action);
            context.RolePermissions.Add(new RolePermission(role.Id, permission.Id));
        }

        await context.SaveChangesAsync();

        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = ValidPassword });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Login did not return an access token.");
        }

        return (user.Id, result.AccessToken);
    }
}
