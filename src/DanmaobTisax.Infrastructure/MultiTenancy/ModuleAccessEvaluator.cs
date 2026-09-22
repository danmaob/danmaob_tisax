using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Infrastructure.MultiTenancy;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public class ModuleAccessEvaluator : IModuleAccessEvaluator
{
    private readonly DanmaobTisaxDbContext _context;

    public ModuleAccessEvaluator(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsModuleEnabledAsync(Guid tenantId, string moduleCode, CancellationToken cancellationToken)
    {
        return await _context.TenantModules
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(m => m.TenantId == tenantId && m.ModuleCode == moduleCode && m.IsEnabled, cancellationToken);
    }
}
