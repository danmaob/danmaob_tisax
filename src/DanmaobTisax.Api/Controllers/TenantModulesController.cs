using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Application.Tenants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform/tenants/{tenantId:guid}/modules")]
[RequirePermission("Platform.ManageTenants")]
public class TenantModulesController : ControllerBase
{
    private readonly ITenantModuleAdministrationService _tenantModuleAdministrationService;
    private readonly IStringLocalizer<TenantModulesController> _localizer;

    public TenantModulesController(ITenantModuleAdministrationService tenantModuleAdministrationService, IStringLocalizer<TenantModulesController> localizer)
    {
        _tenantModuleAdministrationService = tenantModuleAdministrationService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TenantModuleStateDto>>> GetModules(Guid tenantId, CancellationToken cancellationToken)
    {
        var modules = await _tenantModuleAdministrationService.GetModulesAsync(tenantId, cancellationToken);
        if (modules is null)
        {
            return NotFound();
        }
        return Ok(modules);
    }

    [HttpPut("{moduleCode}")]
    public async Task<ActionResult<TenantModuleStateDto>> SetException(Guid tenantId, string moduleCode, [FromBody] SetTenantModuleExceptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _tenantModuleAdministrationService.SetExceptionAsync(tenantId, moduleCode, request.State, cancellationToken);
        if (result.Outcome == TenantModuleOperationOutcome.Succeeded)
        {
            return Ok(result.Value);
        }
        if (result.Outcome == TenantModuleOperationOutcome.TenantNotFound)
        {
            return NotFound();
        }
        if (result.Outcome == TenantModuleOperationOutcome.UnknownModuleCode)
        {
            return BadRequest(BuildProblem(400, _localizer["Errors.TenantModuleUnknownCode"], "Tenant.UnknownModuleCode"));
        }
        if (result.Outcome == TenantModuleOperationOutcome.InvalidState)
        {
            return BadRequest(BuildProblem(400, _localizer["Errors.TenantModuleInvalidState"], "Tenant.InvalidModuleExceptionState"));
        }
        throw new InvalidOperationException("Unexpected outcome: " + result.Outcome);
    }

    private static ProblemDetails BuildProblem(int statusCode, string title, string errorCode)
    {
        var problem = new ProblemDetails { Status = statusCode, Title = title };
        problem.Extensions["errorCode"] = errorCode;
        return problem;
    }
}

public record SetTenantModuleExceptionRequest([Required] string State);
