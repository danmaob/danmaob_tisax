using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.Configuration;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.Identity;

public static class AdminBootstrapper
{
    public static async Task RunAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

         // Validate that required configuration keys are present (just throw if missing)
        RequiredConfigurationValidator.EnsurePresent(configuration, "BootstrapAdmin:TenantId", "Set it with: dotnet user-secrets set \"BootstrapAdmin:TenantId\" \"<value>\"");
        RequiredConfigurationValidator.EnsurePresent(configuration, "BootstrapAdmin:Email", "Set it with: dotnet user-secrets set \"BootstrapAdmin:Email\" \"<email>\"");
        RequiredConfigurationValidator.EnsurePresent(configuration, "BootstrapAdmin:Password", "Set it with: dotnet user-secrets set \"BootstrapAdmin:Password\" \"<password>\"");

        // Get configuration values directly from IConfiguration using null-coalescing
        var tenantId = Guid.Parse(configuration["BootstrapAdmin:TenantId"] ?? string.Empty);
        var email = (configuration["BootstrapAdmin:Email"] ?? string.Empty)!;
        var password = (configuration["BootstrapAdmin:Password"] ?? string.Empty)!;
        var tenantName = configuration["BootstrapAdmin:TenantName"];
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            tenantName = "Default Organization";
        }
        var tenantExists = await context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (tenantExists == false)
        {
            context.Tenants.Add(new Tenant(tenantName!) { Id = tenantId });
            await context.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Tenant row created for TenantId '{tenantId}' with the default plan.");
        }
        // Check if a User already exists for this TenantId + Email combination
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);

        if (existingUser != null)
        {
            Console.WriteLine($"A user already exists for TenantId '{tenantId}' and Email '{email}'. Skipping creation.");
            return;
        }

        // Hash the password
        var passwordHash = passwordHasher.Hash(password);

        // Create the User
        var user = new User(tenantId, email, passwordHash, "Administrator");
        context.Users.Add(user);

        // Create the Administrator Role for this tenant
        var role = new Role(tenantId, "Administrator", null, true);
        context.Roles.Add(role);

        // Create UserRole linking the user and role
        var userRole = new UserRole(user.Id, role.Id);
        context.UserRoles.Add(userRole);

        // Load all Permissions from the catalog and create RolePermission entries
        var permissions = await context.Permissions.ToListAsync(cancellationToken);
        foreach (var permission in permissions)
        {
            var rolePermission = new RolePermission(role.Id, permission.Id);
            context.RolePermissions.Add(rolePermission);
        }

        // Save changes
        await context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Administrator created successfully with email '{email}'.");
    }
}
