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
        var existingCodes = new HashSet<string>(
            await context.Permissions
                .Select(p => p.Module + "." + p.Action)
                .ToListAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);

        var permissionsToInsert = new List<Permission>
        {
            new Permission("Audit", "Read", "View audit log entries"),
            new Permission("Identity", "ManageUsers", "Create, update, deactivate users"),
            new Permission("Identity", "ManageRoles", "Create, update, delete roles"),
            new Permission("Identity", "AssignRoles", "Assign or remove roles from users"),
            new Permission("Tenant", "ManageSettings", "Manage tenant-level configuration"),
            new Permission("Platform", "ManageTenants", "Create, suspend, reactivate and deactivate tenants"),
            new Permission("Platform", "ManagePlans", "Create and configure commercial plans and the module-plan matrix")
        };

        foreach (var permission in permissionsToInsert)
        {
            if (existingCodes.Contains(permission.Code))
            {
                continue;
            }

            context.Permissions.Add(permission);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
