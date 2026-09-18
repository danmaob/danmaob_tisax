using System.Linq;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using DanmaobTisax.Infrastructure.IntegrationTests.Auditing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditLogPersistenceChronologicalTests
{
    [Fact]
    public async Task PersistingAuditableEntityChanges_AreQueryableInChronologicalOrder()
    {
        // Arrange: Build a fake tenant provider (no tenant for test entities that don't implement ITenantOwned)
        var databaseName = $"AuditLogPersistenceChronologicalTests_{Guid.NewGuid()}";
        var fakeTenantProvider = new FakeCurrentTenantProvider();
        // Note: Test entities like AuditableTestEntity don't implement ITenantOwned, so AuditLog will have TenantId = null

        // Create a single context that we'll use for all operations
        using var sharedContext = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: new AuditSaveChangesInterceptor(new FakeCurrentUserService { UserId = Guid.NewGuid(), DisplayName = "User" })
        );

        // Step 1: Create an AuditableTestEntity
        var auditableTestEntity = new AuditableTestEntity { Name = "Entity to audit" };
        sharedContext.AuditableTestEntities.Add(auditableTestEntity);
        await sharedContext.SaveChangesAsync();
        var entityId = auditableTestEntity.Id.ToString();

        // Query the history directly from context (bypassing tenant filter for test entities)
        var auditLogsAfterCreation = sharedContext.AuditLogs.ToList();

        // Verify after creation - assert that there is exactly one audit log row
        Assert.Equal(1, auditLogsAfterCreation.Count);

        // Step 2: Modify the entity (reuse the same context instance)
        auditableTestEntity.Name = "Modified entity";
        await sharedContext.SaveChangesAsync();

        // Query after modification
        var logsAfterUpdate = sharedContext.AuditLogs.ToList();

        // Verify after modification
        Assert.Equal(2, logsAfterUpdate.Count);

        // Step 3: Delete the entity (reuse the same context instance)
        var entityToDelete = sharedContext.AuditableTestEntities.First(e => e.Id == auditableTestEntity.Id);
        sharedContext.AuditableTestEntities.Remove(entityToDelete);
        await sharedContext.SaveChangesAsync();

        // Query after deletion
        var logsAfterDelete = sharedContext.AuditLogs.ToList();

        // Verify after deletion
        Assert.Equal(3, logsAfterDelete.Count);

        // Build AuditLogQueryService and query history using the same shared context
        var queryService = new AuditLogQueryService(sharedContext, fakeTenantProvider);

        // Act & Assert: Get history for the entity directly from context (bypass tenant filter for test entities without TenantId)
        var history = await sharedContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityName == nameof(AuditableTestEntity) && a.EntityId == entityId)
            .OrderBy(a => a.PerformedAtUtc)
            .Take(500)
            .Select(a => new Application.Auditing.AuditLogDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action.ToString(),
                PerformedByUserId = a.PerformedByUserId,
                PerformedByDisplayName = a.PerformedByDisplayName,
                PerformedAtUtc = a.PerformedAtUtc,
                OldValuesJson = a.OldValuesJson,
                NewValuesJson = a.NewValuesJson,
                ChangedColumnsJson = a.ChangedColumnsJson
            })
            .ToListAsync(cancellationToken: default);

        Assert.Equal(3, history.Count);
        Assert.Equal("Created", history[0].Action);
        Assert.Equal("Updated", history[1].Action);
        Assert.Equal("Deleted", history[2].Action);
    }
}
