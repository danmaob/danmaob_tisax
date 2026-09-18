using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Common;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.Persistence;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditInterceptorUpdatedTests
{
    [Fact]
    public async Task ModifyingAuditableEntity_ProducesAuditLogWithOnlyChangedProperties()
    {
        var tenantProvider = new FakeCurrentTenantProvider();
        var userService = new FakeCurrentUserService();
        var interceptor = new AuditSaveChangesInterceptor(userService);

        var firstContext = AuditableTestDbContext.CreateWithInterceptor("AuditDb_Updated", tenantProvider, interceptor);
        
        await firstContext.Database.EnsureCreatedAsync();

        var entity = new AuditableTestEntity { Name = "InitialName" };
        firstContext.AuditableTestEntities.Add(entity);
        await firstContext.SaveChangesAsync();

        var secondContext = AuditableTestDbContext.CreateWithInterceptor("AuditDb_Updated", tenantProvider, interceptor);

        var loadedEntity = await secondContext.AuditableTestEntities.SingleAsync(e => e.Id == entity.Id);
        loadedEntity.Name = "ChangedName";

        await secondContext.SaveChangesAsync();

        var updatedLog = secondContext.AuditLogs.Single(l => l.Action == AuditAction.Updated);
        
        Assert.Equal(AuditAction.Updated, updatedLog.Action);
        Assert.Contains("Name", updatedLog.ChangedColumnsJson);
    }
}
