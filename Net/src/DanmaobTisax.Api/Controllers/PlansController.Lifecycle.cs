using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using DanmaobTisax.Application.Plans;

namespace DanmaobTisax.Api.Controllers;

public partial class PlansController
{
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<PlanDto>> DeactivatePlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.DeactivateAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<PlanDto>> ReactivatePlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.ReactivateAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    [HttpPut("{id:guid}/modules")]
    public async Task<ActionResult<PlanDto>> SetPlanModules(
        Guid id,
        [FromBody] SetPlanModulesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _planAdministrationService.SetModulesAsync(id, request.ModuleCodes, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Outcome);
    }

    public record SetPlanModulesRequest([Required] List<string> ModuleCodes);
}
