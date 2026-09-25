using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using DanmaobTisax.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform/auth")]
public class PlatformAuthController : ControllerBase
{
    private readonly IPlatformAuthenticationService _platformAuthenticationService;

    public PlatformAuthController(IPlatformAuthenticationService platformAuthenticationService)
    {
        _platformAuthenticationService = platformAuthenticationService;
    }

    public record PlatformLoginRequest([Required, EmailAddress] string Email, [Required] string Password);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] PlatformLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _platformAuthenticationService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result.Succeeded == true)
        {
            return Ok(result);
        }

        return Unauthorized(result.FailureReason);
    }
}