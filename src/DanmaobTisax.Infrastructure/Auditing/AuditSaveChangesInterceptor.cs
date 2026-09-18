using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DanmaobTisax.Infrastructure.Auditing;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AuditImmutabilityGuard.Enforce(eventData.Context);

        if (eventData.Context is null)
        {
            return base.SavingChanges(eventData, result);
        }

        var auditLogsToAdd = BuildAuditLogsForPendingChanges(eventData.Context);
        eventData.Context.Set<AuditLog>().AddRange(auditLogsToAdd);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AuditImmutabilityGuard.Enforce(eventData.Context);

        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var auditLogsToAdd = BuildAuditLogsForPendingChanges(eventData.Context);
        eventData.Context.Set<AuditLog>().AddRange(auditLogsToAdd);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> BuildAuditLogsForPendingChanges(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(entry => entry.Entity is IAuditable &&
                            (entry.State == EntityState.Added ||
                             entry.State == EntityState.Modified ||
                             entry.State == EntityState.Deleted))
            .ToList();

        var auditLogsToAdd = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType() == typeof(AuditLog))
            {
                continue;
            }

            var entityName = entry.Entity.GetType().Name;
            var primaryKey = entry.Metadata.FindPrimaryKey();
            var entityId = primaryKey is not null
                ? entry.Property(primaryKey.Properties[0].Name).CurrentValue?.ToString() ?? string.Empty
                : string.Empty;

            AuditLog auditLog;

            if (entry.State == EntityState.Added)
            {
                var newValuesJson = AuditValueSerializer.SerializeAllCurrentValues(entry);
                auditLog = AuditLogFactory.Build(
                    entity: entry.Entity,
                    entityName: entityName,
                    entityId: entityId,
                    action: AuditAction.Created,
                    currentUserService: _currentUserService,
                    oldValuesJson: null,
                    newValuesJson: newValuesJson,
                    changedColumnsJson: null);
            }
            else if (entry.State == EntityState.Modified)
            {
                var oldValuesJson = AuditValueSerializer.SerializeModifiedOriginalValues(entry);
                var newValuesJson = AuditValueSerializer.SerializeModifiedCurrentValues(entry);
                var changedColumnsJson = AuditValueSerializer.SerializeModifiedPropertyNames(entry);
                auditLog = AuditLogFactory.Build(
                    entity: entry.Entity,
                    entityName: entityName,
                    entityId: entityId,
                    action: AuditAction.Updated,
                    currentUserService: _currentUserService,
                    oldValuesJson: oldValuesJson,
                    newValuesJson: newValuesJson,
                    changedColumnsJson: changedColumnsJson);
            }
            else
            {
                var oldValuesJson = AuditValueSerializer.SerializeAllOriginalValues(entry);
                auditLog = AuditLogFactory.Build(
                    entity: entry.Entity,
                    entityName: entityName,
                    entityId: entityId,
                    action: AuditAction.Deleted,
                    currentUserService: _currentUserService,
                    oldValuesJson: oldValuesJson,
                    newValuesJson: null,
                    changedColumnsJson: null);
            }

            auditLogsToAdd.Add(auditLog);
        }

        return auditLogsToAdd;
    }
}
