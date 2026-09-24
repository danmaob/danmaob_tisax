using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Tenants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform/tenants")]
[RequirePermission("Platform.ManageTenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantAdministrationService _tenantAdministrationService;
    private readonly IStringLocalizer<TenantsController> _localizer;

    public TenantsController(
        ITenantAdministrationService tenantAdministrationService,
        IStringLocalizer<TenantsController> localizer)
    {
        _tenantAdministrationService = tenantAdministrationService;
        _localizer = localizer;
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _tenantAdministrationService.CreateAsync(request.Name, cancellationToken);

        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetTenantById), new { id = result.Value!.Id }, result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetTenantById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantAdministrationService.GetByIdAsync(id, cancellationToken);

        if (tenant is null)
        {
            return NotFound();
        }

        return Ok(tenant);
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<TenantDto>> SuspendTenant(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _tenantAdministrationService.SuspendAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<TenantDto>> ReactivateTenant(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _tenantAdministrationService.ReactivateAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<TenantDto>> DeactivateTenant(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _tenantAdministrationService.DeactivateAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpPut("{id:guid}/plan")]
    public async Task<ActionResult<TenantDto>> ChangeTenantPlan(
        Guid id,
        [FromBody] ChangeTenantPlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _tenantAdministrationService.ChangePlanAsync(id, request.PlanId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetTenantAuditHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var history = await _tenantAdministrationService.GetAuditHistoryAsync(id, cancellationToken);

        if (history is null)
        {
            return NotFound();
        }

        return Ok(history);
    }

    private ActionResult<TenantDto> MapFailure(TenantOperationOutcome outcome)
    {
        return outcome switch
        {
            TenantOperationOutcome.NotFound => NotFound(),
            TenantOperationOutcome.InvalidName => BadRequest(BuildProblem(
                400,
                _localizer["Errors.TenantNameInvalid"],
                "Tenant.InvalidName")),
            TenantOperationOutcome.NameAlreadyExists => Conflict(BuildProblem(
                409,
                _localizer["Errors.TenantNameAlreadyExists"],
                "Tenant.NameAlreadyExists")),
            TenantOperationOutcome.InvalidStatusTransition => Conflict(BuildProblem(
                409,
                _localizer["Errors.TenantInvalidStatusTransition"],
                "Tenant.InvalidStatusTransition")),
            TenantOperationOutcome.PlanNotFound => BadRequest(BuildProblem(400, _localizer["Errors.TenantPlanNotFound"], "Tenant.PlanNotFound")),
            TenantOperationOutcome.PlanInactive => Conflict(BuildProblem(409, _localizer["Errors.TenantPlanInactive"], "Tenant.PlanInactive")),
            _ => throw new InvalidOperationException("Unexpected outcome: " + outcome)
        };
    }

    private static ProblemDetails BuildProblem(int statusCode, string title, string errorCode)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };

        problem.Extensions["errorCode"] = errorCode;

        return problem;
    }

    public record CreateTenantRequest([Required, StringLength(200)] string Name);
    public record ChangeTenantPlanRequest([Required] Guid PlanId);
}
