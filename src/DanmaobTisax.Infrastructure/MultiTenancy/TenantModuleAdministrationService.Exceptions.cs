using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public partial class TenantModuleAdministrationService : ITenantModuleAdministrationService
{
    public async Task<TenantModuleOperationResult> SetExceptionAsync(Guid tenantId, string moduleCode, string state, CancellationToken cancellationToken)
    {
        if ((state != TenantModuleExceptionStates.Enabled && state != TenantModuleExceptionStates.Disabled && state != TenantModuleExceptionStates.Inherit) is true)
        {
            return new TenantModuleOperationResult(TenantModuleOperationOutcome.InvalidState, null);
        }

        var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (tenantExists == false)
        {
            return new TenantModuleOperationResult(TenantModuleOperationOutcome.TenantNotFound, null);
        }

        var moduleExists = await _context.FunctionalModules.AnyAsync(m => m.Code == moduleCode, cancellationToken);

        if (moduleExists == false)
        {
            return new TenantModuleOperationResult(TenantModuleOperationOutcome.UnknownModuleCode, null);
        }

        var row = await _context.TenantModules.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.TenantId == tenantId && m.ModuleCode == moduleCode, cancellationToken);

        if (row is null)
        {
            if (state != TenantModuleExceptionStates.Inherit)
            {
                _context.TenantModules.Add(new TenantModule(tenantId, moduleCode, state == TenantModuleExceptionStates.Enabled));
            }
        }
        else if (state == TenantModuleExceptionStates.Enabled)
        {
            row.Enable();
        }
        else if (state == TenantModuleExceptionStates.Disabled)
        {
            row.Disable();
        }
        else
        {
            row.ClearException();
        }

        await _context.SaveChangesAsync(cancellationToken);

        var modules = await GetModulesAsync(tenantId, cancellationToken);
        var value = modules!.First(m => m.ModuleCode == moduleCode);
        return new TenantModuleOperationResult(TenantModuleOperationOutcome.Succeeded, value);
    }
}