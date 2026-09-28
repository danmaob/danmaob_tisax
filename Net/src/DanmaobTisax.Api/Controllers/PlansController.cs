using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Application.Plans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform/plans")]
[RequirePermission("Platform.ManagePlans")]
public partial class PlansController : ControllerBase
{
    private readonly IPlanAdministrationService _planAdministrationService;
    private readonly IStringLocalizer<PlansController> _localizer;

    public PlansController(
        IPlanAdministrationService planAdministrationService,
        IStringLocalizer<PlansController> localizer)
    {
        _planAdministrationService = planAdministrationService;
        _localizer = localizer;
    }

    [HttpGet("module-catalog")]
    public async Task<ActionResult<IReadOnlyList<FunctionalModuleDto>>> GetModuleCatalog(
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.GetModuleCatalogAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> GetPlans(
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlanDto>> GetPlanById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var plan = await _planAdministrationService.GetByIdAsync(id, cancellationToken);

        if (plan is null)
        {
            return NotFound();
        }

        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<PlanDto>> CreatePlan(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.CreateAsync(request.Code, request.Name, cancellationToken);

        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetPlanById), new { id = result.Value!.Id }, result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpPut("{id:guid}/name")]
    public async Task<ActionResult<PlanDto>> RenamePlan(
        Guid id,
        [FromBody] RenamePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.RenameAsync(id, request.Name, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    private ActionResult<PlanDto> MapFailure(PlanOperationOutcome outcome)
    {
        return outcome switch
        {
            PlanOperationOutcome.NotFound => NotFound(),
            PlanOperationOutcome.InvalidCode => BadRequest(BuildProblem(
                400,
                _localizer["Errors.PlanInvalidCode"],
                "Plan.InvalidCode")),
            PlanOperationOutcome.InvalidName => BadRequest(BuildProblem(
                400,
                _localizer["Errors.PlanInvalidName"],
                "Plan.InvalidName")),
            PlanOperationOutcome.UnknownModuleCode => BadRequest(BuildProblem(
                400,
                _localizer["Errors.PlanUnknownModuleCode"],
                "Plan.UnknownModuleCode")),
            PlanOperationOutcome.CodeAlreadyExists => Conflict(BuildProblem(
                409,
                _localizer["Errors.PlanCodeAlreadyExists"],
                "Plan.CodeAlreadyExists")),
            PlanOperationOutcome.PlanInUse => Conflict(BuildProblem(
                409,
                _localizer["Errors.PlanInUse"],
                "Plan.InUse")),
            PlanOperationOutcome.DefaultPlanCannotBeDeactivated => Conflict(BuildProblem(
                409,
                _localizer["Errors.PlanDefaultCannotBeDeactivated"],
                "Plan.DefaultPlanCannotBeDeactivated")),
            PlanOperationOutcome.InvalidStateTransition => Conflict(BuildProblem(
                409,
                _localizer["Errors.PlanInvalidStateTransition"],
                "Plan.InvalidStateTransition")),
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

    public record CreatePlanRequest([Required, StringLength(50)] string Code, [Required, StringLength(100)] string Name);

    public record RenamePlanRequest([Required, StringLength(100)] string Name);
}
