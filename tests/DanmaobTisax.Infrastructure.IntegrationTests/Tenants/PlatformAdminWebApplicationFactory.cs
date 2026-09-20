namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class PlatformAdminWebApplicationFactory : AuthEndpointsWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        string? databaseName = Guid.NewGuid().ToString();

        builder.ConfigureServices(services =>
        {
            // AddInfrastructure registers DanmaobTisaxDbContext against SqlServer using the
            // (IServiceProvider, DbContextOptionsBuilder) overload of AddDbContext. EF Core
            // tracks that provider configuration in internal descriptors keyed by TContext
            // (not just the DbContextOptions<TContext> registration), so simply re-calling
            // AddDbContext with a different provider stacks both configurations instead of
            // replacing them. Remove every descriptor generic over DanmaobTisaxDbContext plus
            // the context registration itself before swapping in the InMemory provider.
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DanmaobTisaxDbContext) ||
                            (d.ServiceType.IsGenericType &&
                             d.ServiceType.GetGenericArguments().Contains(typeof(DanmaobTisaxDbContext))))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Re-register with InMemoryDatabase and interceptor, resolving the interceptor
            // from the provider instead of creating it inline.
            services.AddDbContext<DanmaobTisaxDbContext>((serviceProvider, options) =>
            {
                var context = serviceProvider.GetRequiredService<DanmaobTisaxDbContext>();
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

        string email = Guid.NewGuid() + "@example.com";
        string password = "Str0ng!Passw0rd";
        string hashedPassword = passwordHasher.Hash(password);

        User user = new(TestTenantId, email, hashedPassword, "Platform Test User");
        
        Role role = new(TestTenantId, "PlatformTestRole-" + Guid.NewGuid().ToString(), null, false);

        UserRole userRole = new(user.Id, role.Id);

        context.Users.Add(user);
        context.Roles.Add(role);
        context.UserRoles.Add(userRole);

        if (grantManageTenantsPermission)
        {
            var permission = await context.Permissions.FirstAsync(p => p.Module == "Platform" && p.Action == "ManageTenants");
            context.RolePermissions.Add(new RolePermission(role.Id, permission.Id));
        }

        await context.SaveChangesAsync();

        using HttpClient client = CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResult>();
        
        if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {
            throw new InvalidOperationException("Login did not return an access token.");
        }

        return (user.Id, loginResult.AccessToken);
    }

    public HttpClient CreateClientWithToken(string accessToken)
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
