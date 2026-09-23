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
        if (moduleCodes is null || moduleCodes.Count == 0)
        {
            return new PlanOperationResult(PlanOperationOutcome.UnknownModuleCode, null);
        }

        var requestedCodes = new List<string>();

        foreach (var code in moduleCodes)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new PlanOperationResult(PlanOperationOutcome.UnknownModuleCode, null);
            }

            requestedCodes.Add(code.Trim());
        }

        var distinctCodes = requestedCodes.Distinct(StringComparer.Ordinal).ToList();

        var catalogCodes = await _context.FunctionalModules
            .Select(fm => fm.Code)
            .ToListAsync(cancellationToken);

        foreach (var code in distinctCodes)
        {
            if (!catalogCodes.Contains(code, StringComparer.Ordinal))
            {
                return new PlanOperationResult(PlanOperationOutcome.UnknownModuleCode, null);
            }
        }

        var planWithTracking = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        var existingRows = await _context.PlanModules.ToListAsync(cancellationToken);

        foreach (var code in catalogCodes)
        {
            var row = existingRows.FirstOrDefault(r => r.ModuleCode == code);

            if (distinctCodes.Contains(code))
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
            else
            {
                if (row is not null && row.IsEnabled == true)
                {
                    row.Disable();
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var planFromTrackingContext = existingRows.FirstOrDefault(r => r.PlanId == planId);

        var moduleCodesList = await GetEnabledModuleCodesAsync(planId, cancellationToken);

        // The plan must exist at this point; if it doesn't, treat as not found
        if (planWithTracking is null)
        {
            return new PlanOperationResult(PlanOperationOutcome.NotFound, null);
        }

        return new PlanOperationResult(PlanOperationOutcome.Succeeded, ToDto(planWithTracking, moduleCodesList));
    }
}
