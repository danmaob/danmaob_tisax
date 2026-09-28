using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Auditing;

namespace DanmaobTisax.Infrastructure.Auditing;

public static class AuditImmutabilityGuard
{
    public static void Enforce(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<AuditLog>())
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                throw new AuditLogImmutableException();
            }
        }
    }
}