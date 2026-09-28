using Microsoft.AspNetCore.Hosting;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class BootstrapWebApplicationFactory : PlatformAdminWebApplicationFactory
{
    public static readonly Guid BootstrapTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public const string BootstrapTenantName = "Bootstrap Test Organization";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("BootstrapAdmin:TenantId", BootstrapTenantId.ToString());
        builder.UseSetting("BootstrapAdmin:Email", "bootstrap-admin@example.com");
        builder.UseSetting("BootstrapAdmin:Password", "Str0ng!Passw0rd");
        builder.UseSetting("BootstrapAdmin:TenantName", BootstrapTenantName);
        builder.UseSetting("BootstrapPlatformAdmin:Email", "bootstrap-admin@example.com");
        builder.UseSetting("BootstrapPlatformAdmin:Password", "Str0ng!Passw0rd");
    }
}
