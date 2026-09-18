using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.LayerFilteringTests;

public class TenantIsolationTests
{
    [Fact]
    public async Task Tenant_Cannot_See_Data_From_Another_Tenant()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        // Seed data for both tenants. Inserts are NOT affected by the query
        // filter (the filter only applies to reads), so this works regardless
        // of which tenant the provider is currently set to.
        var seedProvider = new FakeCurrentTenantProvider { Mode = MultiTenancyMode.MultiTenant, CurrentTenantId = tenantA };
        await using (var seedContext = new TestDbContext(options, seedProvider))
        {
            seedContext.TestOnlyNotes.Add(new TestOnlyNote { TenantId = tenantA, Text = "Belongs to tenant A" });
            seedContext.TestOnlyNotes.Add(new TestOnlyNote { TenantId = tenantB, Text = "Belongs to tenant B" });
            await seedContext.SaveChangesAsync();
        }

        // Query as tenant A only.
        var queryProvider = new FakeCurrentTenantProvider { Mode = MultiTenancyMode.MultiTenant, CurrentTenantId = tenantA };
        await using var queryContext = new TestDbContext(options, queryProvider);

        var visibleNotes = await queryContext.TestOnlyNotes.ToListAsync();

        // Tenant isolation ensures that only the current tenant's records are returned.
        Assert.Single(visibleNotes);
        Assert.DoesNotContain(visibleNotes, n => n.TenantId == tenantB);
    }
}