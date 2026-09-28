using DanmaobTisax.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public class CurrentTenantProvider : ICurrentTenantProvider
{
    public MultiTenancyMode Mode { get; }
    public Guid? CurrentTenantId { get; }

    public CurrentTenantProvider(IConfiguration configuration)
    {
        var modeValue = configuration["MultiTenancy:Mode"] ?? nameof(MultiTenancyMode.SingleTenant);

        if (!Enum.TryParse<MultiTenancyMode>(modeValue, ignoreCase: true, out var mode))
        {
            throw new InvalidOperationException(
                $"Invalid MultiTenancy:Mode value '{modeValue}'. Expected 'SingleTenant' or 'MultiTenant'.");
        }

        Mode = mode;

        if (Mode == MultiTenancyMode.SingleTenant)
        {
            var tenantIdValue = configuration["MultiTenancy:FixedTenantId"];
            if (string.IsNullOrWhiteSpace(tenantIdValue) || !Guid.TryParse(tenantIdValue, out var fixedTenantId))
            {
                throw new InvalidOperationException(
                    "MultiTenancy:FixedTenantId must be set to a valid GUID when MultiTenancy:Mode is SingleTenant.");
            }

            CurrentTenantId = fixedTenantId;
        }
        else
        {
            throw new NotSupportedException(
                "MultiTenancy:Mode = MultiTenant is not supported yet. Real per-request tenant " +
                "resolution requires authentication (TS-00-3) and the module/tenant evaluation " +
                "engine (US-20-2), neither of which exists yet. Do not implement a temporary or " +
                "insecure resolution mechanism (for example, trusting an unauthenticated request " +
                "header) — wait until those tasks are complete. See ADR-0002.");
        }
    }
}