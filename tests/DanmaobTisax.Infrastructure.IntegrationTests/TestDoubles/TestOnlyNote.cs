using DanmaobTisax.Domain.Common;

namespace DanmaobTisax.Infrastructure.IntegrationTests.TestDoubles;

public class TestOnlyNote : BaseEntity, ITenantOwned
{
    public Guid TenantId { get; set; }
    public string Text { get; set; } = string.Empty;
}
