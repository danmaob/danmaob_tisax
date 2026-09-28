using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;

public class TestDbContext : DanmaobTisaxDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options, ICurrentTenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }

    public DbSet<TestOnlyNote> TestOnlyNotes => Set<TestOnlyNote>();

    protected override void OnModelCreatingCustom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestOnlyNote>();
    }
}
