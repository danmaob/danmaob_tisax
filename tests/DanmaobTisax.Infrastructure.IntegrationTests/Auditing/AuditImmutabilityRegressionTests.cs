using System.Linq;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using DanmaobTisax.Infrastructure.IntegrationTests.Auditing;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditImmutabilityRegressionTests
{
    [Fact]
    public async Task NormalBusinessWriteWithGeneratedAuditRow_StillSucceeds()
    {
        // Arrange: Build a fake tenant provider, fake user service, and interceptor
        var databaseName = $"AuditImmutabilityRegressionTests_{Guid.NewGuid()}";
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

        // Add a new AuditableTestEntity (which internally causes the interceptor
        // to add a new AuditLog row in 'Added' state — this must NOT be treated
        // as an illegal change)
        var auditableTestEntity = new AuditableTestEntity { Name = "New entity" };
        context.AuditableTestEntities.Add(auditableTestEntity);

        // Act: calling SaveChangesAsync should NOT throw AuditLogImmutableException
        await context.SaveChangesAsync();

        // Assert: exactly one AuditLog row was created
        var auditLogs = context.AuditLogs.ToList();
        Assert.Single(auditLogs);
    }
}
