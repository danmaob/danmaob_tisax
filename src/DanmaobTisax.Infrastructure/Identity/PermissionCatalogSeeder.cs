using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Identity;

public static class PermissionCatalogSeeder
{
    public static async Task SeedAsync(
        DanmaobTisaxDbContext context,
        CancellationToken cancellationToken)
    {
        var existingPermissions = context.Permissions
            .GroupBy(p => p.Module)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Action).First());

        var permissionsToInsert = new List<Permission>
        {
            new Permission("Audit", "Read", "View audit log entries"),
            new Permission("Identity", "ManageUsers", "Create, update, deactivate users"),
            new Permission("Identity", "ManageRoles", "Create, update, delete roles"),
            new Permission("Identity", "AssignRoles", "Assign or remove roles from users"),
            new Permission("Tenant", "ManageSettings", "Manage tenant-level configuration")
        };

        foreach (var permission in permissionsToInsert)
        {
            if (!existingPermissions.TryGetValue(permission.Module, out var existingAction))
            {
                continue;
            }

            if (existingAction != null && string.Equals(existingAction, permission.Action, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            context.Permissions.Add(permission);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
