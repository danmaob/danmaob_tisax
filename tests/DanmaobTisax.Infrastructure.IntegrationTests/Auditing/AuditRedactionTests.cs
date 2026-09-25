using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.IntegrationTests.Identity;
using DanmaobTisax.Infrastructure.IntegrationTests.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditRedactionTests : IClassFixture<PlatformAdminWebApplicationFactory>
{
    private readonly PlatformAdminWebApplicationFactory _factory;

    public AuditRedactionTests(PlatformAdminWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreatingUser_AuditRecordRedactsPasswordHash()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.Hash("Str0ng!Passw0rd");
        var user = new User(AuthEndpointsWebApplicationFactory.TestTenantId, Guid.NewGuid() + "@example.com", passwordHash, "Audit Test User");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var userId = user.Id.ToString();
        var audit = await context.AuditLogs.AsNoTracking().SingleAsync(a => a.EntityName == "User" && a.EntityId == userId && a.Action == AuditAction.Created);
        Assert.NotNull(audit.NewValuesJson);
        Assert.Contains("\"PasswordHash\":\"[REDACTED]\"", audit.NewValuesJson);
        Assert.DoesNotContain(passwordHash, audit.NewValuesJson);
    }

    [Fact]
    public async Task CreatingPlatformAdministrator_AuditRecordRedactsPasswordHash()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.Hash("Str0ng!Passw0rd");
        var administrator = new PlatformAdministrator(Guid.NewGuid() + "@example.com", passwordHash, "Audit Test Administrator");
        context.PlatformAdministrators.Add(administrator);
        await context.SaveChangesAsync();
        var administratorId = administrator.Id.ToString();
        var audit = await context.AuditLogs.AsNoTracking().SingleAsync(a => a.EntityName == "PlatformAdministrator" && a.EntityId == administratorId && a.Action == AuditAction.Created);
        Assert.NotNull(audit.NewValuesJson);
        Assert.Contains("\"PasswordHash\":\"[REDACTED]\"", audit.NewValuesJson);
        Assert.DoesNotContain(passwordHash, audit.NewValuesJson);
    }

    [Fact]
    public async Task ChangingUserPassword_AuditRecordListsColumnAndRedactsBothValues()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User(AuthEndpointsWebApplicationFactory.TestTenantId, Guid.NewGuid() + "@example.com", passwordHasher.Hash("Str0ng!Passw0rd"), "Audit Test User");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        user.ChangePassword(passwordHasher.Hash("An0ther!Passw0rd"));
        await context.SaveChangesAsync();
        var userId = user.Id.ToString();
        var audit = await context.AuditLogs.AsNoTracking().SingleAsync(a => a.EntityName == "User" && a.EntityId == userId && a.Action == AuditAction.Updated);
        Assert.NotNull(audit.ChangedColumnsJson);
        Assert.NotNull(audit.OldValuesJson);
        Assert.NotNull(audit.NewValuesJson);
        Assert.Contains("\"PasswordHash\"", audit.ChangedColumnsJson);
        Assert.Contains("\"PasswordHash\":\"[REDACTED]\"", audit.OldValuesJson);
        Assert.Contains("\"PasswordHash\":\"[REDACTED]\"", audit.NewValuesJson);
    }
}
