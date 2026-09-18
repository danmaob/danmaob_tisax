using System.Linq;
using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using DanmaobTisax.Infrastructure.IntegrationTests.Auditing;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditImmutabilityAbortsWholeOperationTests
{
    [Fact]
    public async Task SaveChangesThatOnlyTouchesAuditLog_AbortsEntireOperation()
    {
        // Arrange: Build a fake tenant provider, fake user service, and interceptor
        var databaseName = $"AuditImmutabilityAbortsWholeOperationTests_{Guid.NewGuid()}";
        var fakeTenantProvider = new FakeCurrentTenantProvider();
        var fakeUserService = new FakeCurrentUserService
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Test User"
        };
        var interceptor = new AuditSaveChangesInterceptor(fakeUserService);

        // Create an AuditableTestDbContext via CreateWithInterceptor with a unique database name
        using var context = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: interceptor
        );

        // Add and save a new AuditableTestEntity - this produces one row in AuditLogs
        var auditableTestEntity = new AuditableTestEntity { Name = "Original entity" };
        context.AuditableTestEntities.Add(auditableTestEntity);
        await context.SaveChangesAsync();

        // Get the AuditLog row ID from the first context
        var auditLogId = context.AuditLogs.First().Id;

        // In a second context over the same database name: add a new Tenant AND modify the AuditLog
        using var mixedContext = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: interceptor
        );

        // Add a new Tenant (legitimate change)
        var newTenant = new Tenant("New tenant");
        mixedContext.Tenants.Add(newTenant);

        // Load the existing AuditLog row and mark it as Modified (illegal change)
        var auditLogRow = mixedContext.AuditLogs.First(a => a.Id == auditLogId);
        mixedContext.Entry(auditLogRow).State = EntityState.Modified;

        // Assert: calling SaveChangesAsync throws AuditLogImmutableException
        await Assert.ThrowsAsync<AuditLogImmutableException>(
            async () => await mixedContext.SaveChangesAsync()
        );

        // In a third, fresh context over the same database name, verify the Tenant was NOT persisted
        using var verificationContext = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: interceptor
        );

        var savedTenants = verificationContext.Tenants.ToList();
        Assert.Empty(savedTenants);
    }
}
