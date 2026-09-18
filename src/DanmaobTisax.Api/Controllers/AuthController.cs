using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICurrentTenantProvider _currentTenantProvider;

    public AuthController(IAuthenticationService authenticationService,
        ICurrentTenantProvider currentTenantProvider)
    {
        _authenticationService = authenticationService;
        _currentTenantProvider = currentTenantProvider;
    }

    public record LoginRequest(string Email, string Password);

    public record RefreshRequest(string RefreshToken);

    public record LogoutRequest(string RefreshToken);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (_currentTenantProvider.CurrentTenantId is null)
        {
            return BadRequest("No se puede realizar la autenticación sin un tenant activo");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authenticationService.LoginAsync(
            _currentTenantProvider.CurrentTenantId.Value,
            request.Email,
            request.Password,
            ipAddress,
            cancellationToken);

        if (result.Succeeded)
        {
            var response = new
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
                FailureReason = result.FailureReason
            };
            return Ok(response);
        }

        return Unauthorized(result.FailureReason);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authenticationService.RefreshAsync(
            request.RefreshToken,
            ipAddress,
            cancellationToken);

        if (result.Succeeded)
        {
            var response = new
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
                FailureReason = result.FailureReason
            };
            return Ok(response);
        }

        return Unauthorized(result.FailureReason);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _authenticationService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return NoContent();
    }
}
