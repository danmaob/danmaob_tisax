namespace DanmaobTisax.Application.Tenants;

public sealed class TenantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public Guid PlanId { get; set; }
}
