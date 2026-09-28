using System.Linq;
using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using DanmaobTisax.Infrastructure.IntegrationTests.Auditing;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditImmutabilityRejectDeleteTests
{
    [Fact]
    public async Task DeletingExistingAuditLog_ThrowsAuditLogImmutableException()
    {
        // Arrange: Build a fake tenant provider, fake user service, and interceptor
        var databaseName = $"AuditImmutabilityRejectDeleteTests_{Guid.NewGuid()}";
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

        // In a second context over the same database name, load that AuditLog row
        using var deleteContext = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: databaseName,
            tenantProvider: fakeTenantProvider,
            interceptor: interceptor
        );
        var auditLogRow = deleteContext.AuditLogs.First();

        // Remove it from the collection
        deleteContext.AuditLogs.Remove(auditLogRow);

        // Assert: calling SaveChangesAsync throws AuditLogImmutableException
        await Assert.ThrowsAsync<AuditLogImmutableException>(
            async () => await deleteContext.SaveChangesAsync()
        );
    }
}
