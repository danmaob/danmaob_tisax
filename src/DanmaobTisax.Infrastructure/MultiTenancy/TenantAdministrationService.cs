using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Exceptions;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public class TenantAdministrationService : ITenantAdministrationService
{
    private readonly DanmaobTisaxDbContext _context;

    public TenantAdministrationService(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    public async Task<TenantOperationResult> CreateAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new TenantOperationResult(TenantOperationOutcome.InvalidName, null);
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length > 200)
        {
            return new TenantOperationResult(TenantOperationOutcome.InvalidName, null);
        }

        string lowerName = trimmedName.ToLower();

        bool alreadyExists = await _context.Tenants.AnyAsync(t => t.Name.ToLower() == lowerName, cancellationToken);

        if (alreadyExists)
        {
            return new TenantOperationResult(TenantOperationOutcome.NameAlreadyExists, null);
        }

        var tenant = new Tenant(trimmedName);

        _context.Tenants.Add(tenant);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = ToDto(tenant);

        return new TenantOperationResult(TenantOperationOutcome.Succeeded, dto);
    }

    public async Task<TenantDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        return ToDto(tenant);
    }

    public async Task<TenantOperationResult> SuspendAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await ApplyTransitionAsync(tenantId, tenant => tenant.Suspend(), cancellationToken);
    }

    private async Task<TenantOperationResult> ApplyTransitionAsync(Guid tenantId, Action<Tenant> transition, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            return new TenantOperationResult(TenantOperationOutcome.NotFound, null);
        }

        try
        {
            transition(tenant);

            await _context.SaveChangesAsync(cancellationToken);

            var dto = ToDto(tenant);

            return new TenantOperationResult(TenantOperationOutcome.Succeeded, dto);
        }
        catch (InvalidTenantStatusTransitionException)
        {
            return new TenantOperationResult(TenantOperationOutcome.InvalidStatusTransition, null);
        }
    }

    public async Task<TenantOperationResult> ReactivateAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await ApplyTransitionAsync(tenantId, tenant => tenant.Reactivate(), cancellationToken);
    }

    public async Task<TenantOperationResult> DeactivateAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await ApplyTransitionAsync(tenantId, tenant => tenant.Deactivate(), cancellationToken);
    }

    public async Task<TenantOperationResult> ChangePlanAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            return new TenantOperationResult(TenantOperationOutcome.NotFound, null);
        }

        if (planId == Guid.Empty)
        {
            return new TenantOperationResult(TenantOperationOutcome.PlanNotFound, null);
        }

        var plan = await _context.Plans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan is null)
        {
            return new TenantOperationResult(TenantOperationOutcome.PlanNotFound, null);
        }

        if (plan.IsActive == false)
        {
            return new TenantOperationResult(TenantOperationOutcome.PlanInactive, null);
        }

        tenant.ChangePlan(planId);

        await _context.SaveChangesAsync(cancellationToken);

        return new TenantOperationResult(TenantOperationOutcome.Succeeded, ToDto(tenant));
    }

    public async Task<IReadOnlyList<AuditLogDto>?> GetAuditHistoryAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        bool exists = await _context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!exists)
        {
            return null;
        }

        string entityId = tenantId.ToString();

        var auditLogs = await _context.AuditLogs.AsNoTracking()
            .Where(a => a.EntityName == nameof(Tenant) && a.EntityId == entityId)
            .OrderBy(a => a.PerformedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = new List<AuditLogDto>();

        foreach (var entry in auditLogs)
        {
            var dto = new AuditLogDto
            {
                Id = entry.Id,
                TenantId = entry.TenantId,
                EntityName = entry.EntityName,
                EntityId = entry.EntityId,
                Action = entry.Action.ToString(),
                PerformedByUserId = entry.PerformedByUserId,
                PerformedByDisplayName = entry.PerformedByDisplayName,
                PerformedAtUtc = entry.PerformedAtUtc,
                OldValuesJson = entry.OldValuesJson,
                NewValuesJson = entry.NewValuesJson,
                ChangedColumnsJson = entry.ChangedColumnsJson
            };

            dtos.Add(dto);
        }

        return dtos.AsReadOnly();
    }

    private static TenantDto ToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = tenant.Status.ToString(),
            CreatedAtUtc = tenant.CreatedAtUtc,
            PlanId = tenant.PlanId
        };
    }
}
