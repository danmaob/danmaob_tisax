using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public partial class TenantModuleAdministrationService
{
    private readonly DanmaobTisaxDbContext _context;

    public TenantModuleAdministrationService(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TenantModuleStateDto>?> GetModulesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var planId = await _context.Tenants.AsNoTracking().Where(t => t.Id == tenantId).Select(t => (Guid?)t.PlanId).FirstOrDefaultAsync(cancellationToken);
        if (planId is null)
        {
            return null;
        }
        var catalogCodes = await _context.FunctionalModules.AsNoTracking().OrderBy(m => m.SortOrder).Select(m => m.Code).ToListAsync(cancellationToken);
        var planEnabledCodes = await _context.PlanModules.AsNoTracking().Where(pm => pm.PlanId == planId.Value && pm.IsEnabled).Select(pm => pm.ModuleCode).ToListAsync(cancellationToken);
        var exceptions = await _context.TenantModules.IgnoreQueryFilters().AsNoTracking().Where(m => m.TenantId == tenantId).ToListAsync(cancellationToken);
        var result = new List<TenantModuleStateDto>();
        foreach (var code in catalogCodes)
        {
            var enabledByPlan = planEnabledCodes.Contains(code);
            var exceptionIsEnabled = exceptions.FirstOrDefault(e => e.ModuleCode == code)?.IsEnabled;
            var isEnabled = exceptionIsEnabled ?? enabledByPlan;
            result.Add(new TenantModuleStateDto(code, enabledByPlan, exceptionIsEnabled, isEnabled));
        }
        return result;
    }
}
