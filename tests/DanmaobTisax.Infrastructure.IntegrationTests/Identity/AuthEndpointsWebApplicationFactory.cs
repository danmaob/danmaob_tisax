using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public class AuthEndpointsWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string TestSigningKey = "test-signing-key-at-least-32-characters-long-for-hmac-sha256";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=placeholder;Database=placeholder;");
        builder.UseSetting("Jwt:Issuer", "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("Jwt:SigningKey", TestSigningKey);
        builder.UseSetting("MultiTenancy:Mode", "SingleTenant");
        builder.UseSetting("MultiTenancy:FixedTenantId", TestTenantId.ToString());

        // DbContextOptions<DanmaobTisaxDbContext> is scoped, so the UseInMemoryDatabase name
        // must be fixed once here and reused on every ConfigureServices/options evaluation
        // (which EF Core re-runs per scope) — otherwise each scope would get its own empty
        // database instead of sharing one across the WebApplicationFactory instance.
        var inMemoryDatabaseName = Guid.NewGuid().ToString();

        builder.ConfigureServices(services =>
        {
            // AddInfrastructure registers DanmaobTisaxDbContext against SqlServer using the
            // (IServiceProvider, DbContextOptionsBuilder) overload of AddDbContext. EF Core
            // tracks that provider configuration in internal descriptors keyed by TContext
            // (not just the DbContextOptions<TContext> registration), so simply re-calling
            // AddDbContext with a different provider stacks both configurations instead of
            // replacing them. Remove every descriptor generic over DanmaobTisaxDbContext plus
            // the context registration itself before swapping in the InMemory provider.
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DanmaobTisaxDbContext) ||
                            (d.ServiceType.IsGenericType &&
                             d.ServiceType.GetGenericArguments().Contains(typeof(DanmaobTisaxDbContext))))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DanmaobTisaxDbContext>(options =>
                options.UseInMemoryDatabase(inMemoryDatabaseName));
        });
    }
}
