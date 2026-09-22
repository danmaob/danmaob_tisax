using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class ModuleGateProbeController : ControllerBase
{
    private const string GatedModuleCode = "Probe.Module";

    [HttpGet("gated")]
    [RequireModule(GatedModuleCode)]
    public IActionResult Gated() => Ok();
}
