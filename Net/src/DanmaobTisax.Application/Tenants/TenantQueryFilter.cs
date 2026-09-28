using DanmaobTisax.Domain.Tenants;

namespace DanmaobTisax.Application.Tenants;

public sealed class TenantQueryFilter
{
	public TenantStatus? Status { get; init; }
	public string? Name { get; init; }
	public int PageNumber { get; init; } = 1;
	public int PageSize { get; init; } = 50;
}
