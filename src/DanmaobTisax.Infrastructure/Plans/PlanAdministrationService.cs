using DanmaobTisax.Application.Plans;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Plans;

public partial class PlanAdministrationService : IPlanAdministrationService
{
    private readonly DanmaobTisaxDbContext _context;

    public PlanAdministrationService(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    private static PlanDto ToDto(Plan plan, IEnumerable<string> moduleCodes)
    {
        var sortedModuleCodes = moduleCodes.OrderBy(m => m, StringComparer.Ordinal).ToList();

        return new PlanDto
        {
            Id = plan.Id,
            Code = plan.Code,
            Name = plan.Name,
            IsActive = plan.IsActive,
            ModuleCodes = sortedModuleCodes
        };
    }

    private async Task<List<string>> GetEnabledModuleCodesAsync(Guid planId, CancellationToken cancellationToken)
    {
        var moduleCodes = await _context.PlanModules.AsNoTracking()
            .Where(pm => pm.PlanId == planId && pm.IsEnabled)
            .Select(pm => pm.ModuleCode)
            .ToListAsync(cancellationToken);

        return moduleCodes;
    }

    public async Task<IReadOnlyList<FunctionalModuleDto>> GetModuleCatalogAsync(CancellationToken cancellationToken)
    {
        var modules = await _context.FunctionalModules.AsNoTracking()
            .OrderBy(fm => fm.SortOrder)
            .Select(fm => new FunctionalModuleDto
            {
                Code = fm.Code,
                SortOrder = fm.SortOrder
            })
            .ToListAsync(cancellationToken);

        return modules;
    }

    public async Task<IReadOnlyList<PlanDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var plans = await _context.Plans.AsNoTracking()
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);

        var enabledModules = await _context.PlanModules.AsNoTracking()
            .Where(pm => pm.IsEnabled)
            .ToListAsync(cancellationToken);

        return plans.Select(plan => ToDto(plan, enabledModules.Where(pm => pm.PlanId == plan.Id).Select(pm => pm.ModuleCode))).ToList();
    }

    public async Task<PlanDto?> GetByIdAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan is null)
        {
            return null;
        }

        var moduleCodes = await GetEnabledModuleCodesAsync(planId, cancellationToken);

        return ToDto(plan, moduleCodes);
    }

    public async Task<PlanOperationResult> CreateAsync(string code, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidCode, null);
        }

        var trimmedCode = code.Trim();

        if (trimmedCode.Length > 50)
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidCode, null);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidName, null);
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > 100)
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidName, null);
        }

        var normalizedCode = trimmedCode.ToUpperInvariant();

        bool alreadyExists = await _context.Plans.AnyAsync(p => p.Code == normalizedCode, cancellationToken);

        if (alreadyExists)
        {
            return new PlanOperationResult(PlanOperationOutcome.CodeAlreadyExists, null);
        }

        var plan = new Plan(trimmedCode, trimmedName);

        _context.Plans.Add(plan);

        await _context.SaveChangesAsync(cancellationToken);

        var moduleCodes = await GetEnabledModuleCodesAsync(plan.Id, cancellationToken);

        return new PlanOperationResult(PlanOperationOutcome.Succeeded, ToDto(plan, moduleCodes));
    }

    public async Task<PlanOperationResult> RenameAsync(Guid planId, string name, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan is null)
        {
            return new PlanOperationResult(PlanOperationOutcome.NotFound, null);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
        {
            return new PlanOperationResult(PlanOperationOutcome.InvalidName, null);
        }

        plan.Rename(name.Trim());

        await _context.SaveChangesAsync(cancellationToken);

        var moduleCodes = await GetEnabledModuleCodesAsync(planId, cancellationToken);

        return new PlanOperationResult(PlanOperationOutcome.Succeeded, ToDto(plan, moduleCodes));
    }
}
