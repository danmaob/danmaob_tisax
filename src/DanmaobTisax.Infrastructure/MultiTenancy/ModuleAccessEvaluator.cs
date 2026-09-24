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
        var exceptionRow = await _context.TenantModules.IgnoreQueryFilters().AsNoTracking().Where(m => m.TenantId == tenantId && m.ModuleCode == moduleCode).Select(m => (bool?)m.IsEnabled).FirstOrDefaultAsync(cancellationToken);
        if (exceptionRow.HasValue)
        {
            return exceptionRow.Value;
        }
        var planId = await _context.Tenants.AsNoTracking().Where(t => t.Id == tenantId).Select(t => (Guid?)t.PlanId).FirstOrDefaultAsync(cancellationToken);
        if (planId == null)
        {
            return false;
        }
        return await _context.PlanModules.AsNoTracking().AnyAsync(pm => pm.PlanId == planId.Value && pm.ModuleCode == moduleCode && pm.IsEnabled, cancellationToken);
    }
}
