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
        var existingUser = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);

        if (existingUser != null)
        {
            var adminRole = await context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == "Administrator" && r.IsSystemRole, cancellationToken);
            if (adminRole is null)
            {
                Console.WriteLine($"A user already exists for TenantId '{tenantId}' and Email '{email}', but no Administrator role was found. No permissions were granted.");
                return;
            }
            var grantedPermissionIds = await context.RolePermissions.Where(rp => rp.RoleId == adminRole.Id).Select(rp => rp.PermissionId).ToListAsync(cancellationToken);
            var missingPermissions = await context.Permissions.Where(p => grantedPermissionIds.Contains(p.Id) == false && p.Module != "Platform").ToListAsync(cancellationToken);
            for (int i = 0; i < missingPermissions.Count; i++)
            {
                var permission = missingPermissions[i];
                var rolePermission = new RolePermission(adminRole.Id, permission.Id);
                context.RolePermissions.Add(rolePermission);
            }
            await context.SaveChangesAsync(cancellationToken);
            var platformPermissionIds = await context.Permissions.Where(p => p.Module == "Platform").Select(p => p.Id).ToListAsync(cancellationToken);
            var platformGrants = await context.RolePermissions.Where(rp => rp.RoleId == adminRole.Id && platformPermissionIds.Contains(rp.PermissionId)).ToListAsync(cancellationToken);
            context.RolePermissions.RemoveRange(platformGrants);
            Console.WriteLine($"A user already exists for TenantId '{tenantId}' and Email '{email}'. Skipping creation. {missingPermissions.Count} missing permission(s) granted to the Administrator role.");
            Console.WriteLine($"{platformGrants.Count} platform permission grant(s) removed from the Administrator role.");
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
        var permissions = await context.Permissions.Where(p => p.Module != "Platform").ToListAsync(cancellationToken);
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
