using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditInterceptorNonAuditableTests
{
    [Fact]
    public async Task ChangingNonAuditableEntity_ProducesNoAuditLog()
    {
        // Arrange
        var tenantProvider = new FakeCurrentTenantProvider();
        var fakeUserService = new FakeCurrentUserService
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Test User"
        };
        var interceptor = new AuditSaveChangesInterceptor(fakeUserService);

        using var context = AuditableTestDbContext.CreateWithInterceptor(
            databaseName: Guid.NewGuid().ToString(),
            tenantProvider: tenantProvider,
            interceptor: interceptor
        );
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var permission = new Permission("Test", "NonAuditable", null);
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(0, context.AuditLogs.Count());
    }
}
