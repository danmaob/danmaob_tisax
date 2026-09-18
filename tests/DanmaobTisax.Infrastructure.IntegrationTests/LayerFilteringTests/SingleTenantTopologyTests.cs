using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.LayerFilteringTests;

public class SingleTenantTopologyTests
{
    [Fact]
    public async Task Same_DbContext_Works_Unmodified_In_SingleTenant_Mode()
    {
        var databaseName = Guid.NewGuid().ToString();
        var fixedTenantId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var provider = new FakeCurrentTenantProvider { Mode = MultiTenancyMode.SingleTenant, CurrentTenantId = fixedTenantId };

        await using (var context = new TestDbContext(options, provider))
        {
            context.TestOnlyNotes.Add(new TestOnlyNote { TenantId = fixedTenantId, Text = "Single-tenant installation note" });
            await context.SaveChangesAsync();
        }

        await using var queryContext = new TestDbContext(options, provider);
        var notes = await queryContext.TestOnlyNotes.ToListAsync();

        Assert.Single(notes);
        Assert.Equal(fixedTenantId, notes[0].TenantId);

        // The important assertion for AC2 is structural, not behavioral: this
        // test uses the exact same TestDbContext class, the exact same model,
        // and the exact same ApplyTenantQueryFilters mechanism as
        // TenantIsolationTests (the MultiTenant test) in the previous prompt.
        // There is no `if (Mode == SingleTenant)` branch, no subclass per
        // topology, and no schema difference anywhere in DanmaobTisaxDbContext
        // to make this work — only the ICurrentTenantProvider values differ.
        // Do not add any such branch to DanmaobTisaxDbContext to "make this
        // test pass more explicitly" — that would violate AC2, not satisfy it.
    }
}