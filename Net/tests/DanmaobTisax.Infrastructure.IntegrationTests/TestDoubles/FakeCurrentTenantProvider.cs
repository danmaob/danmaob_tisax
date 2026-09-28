using DanmaobTisax.Application.Interfaces;

namespace DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;

public class FakeCurrentTenantProvider : ICurrentTenantProvider
{
    public MultiTenancyMode Mode { get; set; } = MultiTenancyMode.MultiTenant;
    public Guid? CurrentTenantId { get; set; }
}

public class ScopedFakeCurrentTenantProvider : ICurrentTenantProvider
{
    public MultiTenancyMode Mode => MultiTenancyMode.SingleTenant;
    public Guid? CurrentTenantId { get; set; }
}
