using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test-probe")]
public class ModuleGateProbeController : ControllerBase
{
    public const string GatedModuleCode = "Probe.Module";

    [HttpGet("gated")]
    [RequireModule(GatedModuleCode)]
    public IActionResult Gated() => Ok();
}
