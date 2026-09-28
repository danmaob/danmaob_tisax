using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public class TenantStatusEvaluator : ITenantStatusEvaluator
{
    private readonly DanmaobTisaxDbContext _context;

    public TenantStatusEvaluator(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsTenantBlockedAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _context.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Id == tenantId && t.Status != TenantStatus.Active, cancellationToken);
    }
}
