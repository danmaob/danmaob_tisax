using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Auditing;
using DanmaobTisax.Infrastructure.Persistence;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Auditing;

public class AuditableTestEntity : BaseEntity, IAuditable
{
    public string Name { get; set; } = string.Empty;
}

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? DisplayName { get; set; }
}

public class AuditableTestDbContext : DanmaobTisaxDbContext
{
    public DbSet<AuditableTestEntity> AuditableTestEntities => Set<AuditableTestEntity>();

    public AuditableTestDbContext(DbContextOptions<AuditableTestDbContext> options, ICurrentTenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }

    protected override void OnModelCreatingCustom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditableTestEntity>();
    }

    public static AuditableTestDbContext CreateWithInterceptor(string databaseName, ICurrentTenantProvider tenantProvider, AuditSaveChangesInterceptor interceptor)
    {
        var builder = new DbContextOptionsBuilder<AuditableTestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(interceptor);

        return new AuditableTestDbContext(builder.Options, tenantProvider);
    }
}
