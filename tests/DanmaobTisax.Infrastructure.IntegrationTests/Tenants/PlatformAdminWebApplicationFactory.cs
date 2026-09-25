using System.Net.Http.Headers;
using System.Net.Http.Json;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Application.Plans;
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
        if (grantManageTenantsPermission == true)
        {
            return await CreatePlatformAdministratorAndLoginAsync();
        }

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

    public async Task<(Guid UserId, string AccessToken)> CreateUserWithPlanManagementAndLoginAsync()
    {
        return await CreatePlatformAdministratorAndLoginAsync();
    }

    public async Task<(Guid UserId, string AccessToken)> CreatePlatformAdministratorAndLoginAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var email = Guid.NewGuid() + "@example.com";
        const string password = "Str0ng!Passw0rd";
        var administrator = new PlatformAdministrator(email, passwordHasher.Hash(password), "Platform Test Administrator");
        context.PlatformAdministrators.Add(administrator);
        await context.SaveChangesAsync();
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Platform login did not return an access token.");
        }

        return (administrator.Id, result.AccessToken);
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

    public async Task<(Guid UserId, string AccessToken)> CreateUserWithPlatformPermissionsAndLoginAsync(IReadOnlyList<string> platformActions)
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

        await context.SaveChangesAsync();

        foreach (var action in platformActions)
        {
            var permission = await context.Permissions.FirstAsync(p => p.Module == "Platform" && p.Action == action);
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

    public async Task<PlanDto> CreatePlanAsync(HttpClient client, IReadOnlyList<string>? moduleCodes = null)
    {
        var code = "T" + Guid.NewGuid().ToString("N").ToUpperInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/platform/plans", new { Code = code, Name = "Plan " + code });
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var plan = await response.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(plan);

        if (moduleCodes is null)
        {
            return plan;
        }

        var updateResponse = await client.PutAsJsonAsync("/api/v1/platform/plans/" + plan.Id + "/modules", new { ModuleCodes = moduleCodes });
        Assert.Equal(System.Net.HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedPlan = await updateResponse.Content.ReadFromJsonAsync<PlanDto>();
        Assert.NotNull(updatedPlan);

        return updatedPlan;
    }

    public Task<HttpResponseMessage> ChangeTenantPlanAsync(HttpClient client, Guid tenantId, Guid planId)
    {
        return client.PutAsJsonAsync("/api/v1/platform/tenants/" + tenantId + "/plan", new { PlanId = planId });
    }
}
