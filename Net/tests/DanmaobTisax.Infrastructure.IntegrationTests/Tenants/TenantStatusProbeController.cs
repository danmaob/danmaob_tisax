using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test-probe-status")]
public class TenantStatusProbeController : ControllerBase
{
    [HttpGet("open")]
    public IActionResult Open()
    {
        return Ok();
    }
}
