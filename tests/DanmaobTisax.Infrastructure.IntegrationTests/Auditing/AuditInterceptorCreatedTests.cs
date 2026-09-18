using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditInterceptorCreatedTests
{
    [Fact]
    public async Task CreatingAuditableEntity_ProducesAuditLogWithCreatedAction()
    {
        // Arrange
        var tenantProvider = new FakeCurrentTenantProvider();
        var fakeUserService = new FakeCurrentUserService
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Test User"
        };
        var interceptor = new AuditSaveChangesInterceptor(fakeUserService);

        // Act
        using var context = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: Guid.NewGuid().ToString(),
            tenantProvider: tenantProvider,
            interceptor: interceptor
        );

        context.AuditableTestEntities.Add(new AuditableTestEntity { Name = "Test Entity" });
        await context.SaveChangesAsync();

        // Assert
        var auditLogs = context.AuditLogs.ToList();

        Assert.Single(auditLogs);
        Assert.Equal(AuditAction.Created, auditLogs[0].Action);
        Assert.Null(auditLogs[0].OldValuesJson);
        Assert.NotNull(auditLogs[0].NewValuesJson);
    }
}
