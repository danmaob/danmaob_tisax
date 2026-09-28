using System.Linq;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using DanmaobTisax.Infrastructure.IntegrationTests.Auditing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditLogTamperLeavesDataUnchangedTests
{
    [Fact]
    public async Task AttemptingToTamperAuditLog_LeavesDataUnchangedWhenQueriedAfterward()
    {
        // Arrange: Build a fake tenant provider (no tenant for test entities)
        var databaseName = $"AuditLogTamperLeavesDataUnchangedTests_{Guid.NewGuid()}";
        var fakeTenantProvider = new FakeCurrentTenantProvider();

        // Create first context to add entity and generate audit log
        using var context1 = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: new AuditSaveChangesInterceptor(new FakeCurrentUserService { UserId = Guid.NewGuid(), DisplayName = "User" })
        );

        // Step 1: Add and save a new AuditableTestEntity - produces one AuditLog row
        var auditableTestEntity = new AuditableTestEntity { Name = "Entity to audit for tamper test" };
        context1.AuditableTestEntities.Add(auditableTestEntity);
        await context1.SaveChangesAsync();
        var entityId = auditableTestEntity.Id.ToString();

        // Step 2: In a second context, load the AuditLog row and capture its PerformedByDisplayName
        using var context2 = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: new AuditSaveChangesInterceptor(new FakeCurrentUserService { UserId = Guid.NewGuid(), DisplayName = "AnotherUser" })
        );

        // Query the audit log directly from context2 (use AsNoTracking since we just need the data)
        var auditLog = await context2.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EntityName == nameof(AuditableTestEntity) && a.EntityId == entityId);

        // Capture the original PerformedByDisplayName value for later comparison
        string? capturedPerformedByDisplayName = auditLog?.PerformedByDisplayName;
        
        // Attempt to modify the tracked audit log - should throw AuditLogImmutableException before saving
        var entry = context2.Entry<AuditLog>(auditLog!);
        Assert.NotNull(entry);

        // Set the state to Modified and attempt SaveChangesAsync - should throw exception
        entry.State = EntityState.Modified;
        
        // Act & Assert: Attempting to save modified audit log should throw exception
        await Assert.ThrowsAsync<AuditLogImmutableException>(() => context2.SaveChangesAsync());

        // Step 3: In a third, fresh context, query the history and verify data is unchanged
        using var context3 = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: new AuditSaveChangesInterceptor(new FakeCurrentUserService { UserId = Guid.NewGuid(), DisplayName = "ThirdUser" })
        );

        // Build AuditLogQueryService and query history for the entity
        var queryService = new AuditLogQueryService(context3, fakeTenantProvider);
        var history = await queryService.GetHistoryForEntityAsync(entityName: nameof(AuditableTestEntity), entityId: entityId, cancellationToken: default);

        // Assert the single returned row's PerformedByDisplayName still equals the captured value
        Assert.Single(history);
        Assert.Equal(capturedPerformedByDisplayName, history[0].PerformedByDisplayName);
    }
}
