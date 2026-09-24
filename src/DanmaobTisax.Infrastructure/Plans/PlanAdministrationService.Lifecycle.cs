using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DanmaobTisax.Domain.Exceptions;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Application.Plans;
using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Infrastructure.Persistence;

namespace DanmaobTisax.Infrastructure.Plans;

public partial class PlanAdministrationService
{
    public async Task<PlanOperationResult> DeactivateAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan is null)
        {
            return new PlanOperationResult(PlanOperationOutcome.NotFound, null);
        }

        if (plan.Id == PlanCatalog.FreePlanId)
        {
            return new PlanOperationResult(PlanOperationOutcome.DefaultPlanCannotBeDeactivated, null);
        }

        if (plan.IsActive == false)
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidStateTransition, null);
        }

        bool hasActiveTenants = await _context.Tenants.AnyAsync(t => t.PlanId == planId && t.Status != TenantStatus.Deactivated, cancellationToken);

        if (hasActiveTenants)
        {
            return new PlanOperationResult(PlanOperationOutcome.PlanInUse, null);
        }

        try
        {
            plan.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            var moduleCodes = await GetEnabledModuleCodesAsync(planId, cancellationToken);

            return new PlanOperationResult(PlanOperationOutcome.Succeeded, ToDto(plan, moduleCodes));
        }
        catch (InvalidPlanStateException)
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidStateTransition, null);
        }
    }

    public async Task<PlanOperationResult> ReactivateAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan is null)
        {
            return new PlanOperationResult(PlanOperationOutcome.NotFound, null);
        }

        try
        {
            plan.Reactivate();
            await _context.SaveChangesAsync(cancellationToken);

            var moduleCodes = await GetEnabledModuleCodesAsync(planId, cancellationToken);

            return new PlanOperationResult(PlanOperationOutcome.Succeeded, ToDto(plan, moduleCodes));
        }
        catch (InvalidPlanStateException)
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidStateTransition, null);
        }
    }

    public async Task<PlanOperationResult> SetModulesAsync(Guid planId, IReadOnlyList<string> moduleCodes, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan is null)
        {
            return new PlanOperationResult(PlanOperationOutcome.NotFound, null);
        }

        if (moduleCodes.Any(string.IsNullOrWhiteSpace))
        {
            return new PlanOperationResult(PlanOperationOutcome.UnknownModuleCode, null);
        }

        var requestedCodes = moduleCodes.Select(c => c.Trim()).Distinct(StringComparer.Ordinal).ToList();

        var catalogCodes = await _context.FunctionalModules.Select(fm => fm.Code).ToListAsync(cancellationToken);

        if (requestedCodes.Any(c => catalogCodes.Contains(c, StringComparer.Ordinal) == false))
        {
            return new PlanOperationResult(PlanOperationOutcome.UnknownModuleCode, null);
        }

        var existingRows = await _context.PlanModules.Where(pm => pm.PlanId == planId).ToListAsync(cancellationToken);

        foreach (var code in catalogCodes)
        {
            var row = existingRows.FirstOrDefault(r => r.ModuleCode == code);
            var isRequested = requestedCodes.Contains(code, StringComparer.Ordinal);

            if (isRequested == true)
            {
                if (row is null)
                {
                    _context.PlanModules.Add(new PlanModule(planId, code, true));
                }
                else if (row.IsEnabled == false)
                {
                    row.Enable();
                }
            }
            else if (row is not null && row.IsEnabled == true)
            {
                row.Disable();
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var enabledCodes = await GetEnabledModuleCodesAsync(planId, cancellationToken);

        return new PlanOperationResult(PlanOperationOutcome.Succeeded, ToDto(plan, enabledCodes));
    }
}
