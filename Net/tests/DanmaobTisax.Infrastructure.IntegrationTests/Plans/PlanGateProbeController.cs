namespace DanmaobTisax.Infrastructure.IntegrationTests.Plans;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Domain.Plans;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test-plan-probe")]
public class PlanGateProbeController : ControllerBase
{
    [HttpGet("evidence")]
    [RequireModule(FunctionalModuleCodes.Evidence)]
    public IActionResult Evidence() => Ok();
}
