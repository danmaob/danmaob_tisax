using System.Net.Http.Headers;
using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class PlatformAdminWebApplicationFactory : AuthEndpointsWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        var databaseName = Guid.NewGuid().ToString();

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DanmaobTisaxDbContext) ||
                            (d.ServiceType.IsGenericType &&
                             d.ServiceType.GetGenericArguments().Contains(typeof(DanmaobTisaxDbContext))))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DanmaobTisaxDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
            });
        });
    }

    public async Task<(Guid UserId, string AccessToken)> CreateUserAndLoginAsync(bool grantManageTenantsPermission)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = Guid.NewGuid() + "@example.com";
        const string password = "Str0ng!Passw0rd";

        var user = new User(TestTenantId, email, passwordHasher.Hash(password), "Platform Test User");
        var role = new Role(TestTenantId, "PlatformTestRole-" + Guid.NewGuid(), null, false);
        var userRole = new UserRole(user.Id, role.Id);

        context.Users.Add(user);
        context.Roles.Add(role);
        context.UserRoles.Add(userRole);

        if (grantManageTenantsPermission)
        {
            var permission = await context.Permissions.FirstAsync(p => p.Module == "Platform" && p.Action == "ManageTenants");
            context.RolePermissions.Add(new RolePermission(role.Id, permission.Id));
        }

        await context.SaveChangesAsync();

        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Login did not return an access token.");
        }

        return (user.Id, result.AccessToken);
    }

    public HttpClient CreateClientWithToken(string accessToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public async Task<TenantDto> CreateTenantAsync(HttpClient client, string? name = null)
    {
        var tenantName = name ?? "Tenant-" + Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/api/v1/platform/tenants", new { Name = tenantName });
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
        Assert.NotNull(tenant);

        return tenant;
    }
}
