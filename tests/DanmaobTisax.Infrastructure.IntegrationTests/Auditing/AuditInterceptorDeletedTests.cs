using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.Persistence;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditInterceptorDeletedTests
{
    [Fact]
    public async Task DeletingAuditableEntity_ProducesAuditLogWithDeletedAction()
    {
        // Arrange
        var tenantProvider = new FakeCurrentTenantProvider();
        var userService = new FakeCurrentUserService();
        var interceptor = new AuditSaveChangesInterceptor(userService);

        var firstContext = AuditableTestDbContext.CreateWithInterceptor("AuditDb_Deleted", tenantProvider, interceptor);
        
        await firstContext.Database.EnsureCreatedAsync();

        var entity = new AuditableTestEntity { Name = "Test Entity" };
        firstContext.AuditableTestEntities.Add(entity);
        await firstContext.SaveChangesAsync();

        var secondContext = AuditableTestDbContext.CreateWithInterceptor("AuditDb_Deleted", tenantProvider, interceptor);

        // Act - Load the entity, remove it from DbSet, and save changes
        var loadedEntity = await secondContext.AuditableTestEntities.SingleAsync(e => e.Id == entity.Id);
        secondContext.AuditableTestEntities.Remove(loadedEntity);
        await secondContext.SaveChangesAsync();

        // Assert
        var deleteLog = secondContext.AuditLogs.Single(l => l.Action == AuditAction.Deleted);

        Assert.Equal(AuditAction.Deleted, deleteLog.Action);
        Assert.NotNull(deleteLog.OldValuesJson);
        Assert.Null(deleteLog.NewValuesJson);
    }
}
