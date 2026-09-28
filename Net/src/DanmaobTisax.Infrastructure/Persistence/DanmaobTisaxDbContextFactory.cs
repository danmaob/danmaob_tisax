using DanmaobTisax.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DanmaobTisax.Infrastructure.Persistence;

/// <summary>
/// Used ONLY by EF Core command-line tooling (dotnet ef migrations add / database
/// update) to construct a DbContext instance without running the full application.
/// Never used at application runtime — the real context is built via dependency
/// injection in AddInfrastructure(). Reads the connection string from the
/// ConnectionStrings__DefaultConnection environment variable (not from User
/// Secrets, which this tooling does not read) — the human operator sets this
/// environment variable in their own shell before running dotnet ef commands.
/// See ADR-0003.
/// </summary>
public class DanmaobTisaxDbContextFactory : IDesignTimeDbContextFactory<DanmaobTisaxDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__DefaultConnection";

    public DanmaobTisaxDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Missing required environment variable: {ConnectionStringEnvironmentVariable}. " +
                "This is needed only for EF Core design-time tooling (dotnet ef migrations add / " +
                "database update). Set it in your shell before running dotnet ef commands, for example: " +
                $"export {ConnectionStringEnvironmentVariable}=\"<your real connection string>\". " +
                "This message intentionally does not include any connection string value.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<DanmaobTisaxDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new DanmaobTisaxDbContext(optionsBuilder.Options, new DesignTimeCurrentTenantProvider());
    }

    private sealed class DesignTimeCurrentTenantProvider : ICurrentTenantProvider
    {
        public MultiTenancyMode Mode => MultiTenancyMode.SingleTenant;
        public Guid? CurrentTenantId => Guid.Empty;
    }
}
