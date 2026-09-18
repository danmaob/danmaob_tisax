using DanmaobTisax.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DanmaobTisax.Infrastructure.Identity;

/// <summary>
/// Implements ICurrentUserService using IHttpContextAccessor for HTTP context awareness.
/// This class is registered in the Infrastructure project's DI container.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Gets the current user's ID if authenticated, otherwise null.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            var subClaim = User?.FindFirst(x => x.Type == "sub")?.Value;
            return Guid.TryParse(subClaim, out var userId) ? userId : null;
        }
    }

    /// <summary>
    /// Gets the current user's display name if authenticated.
    /// </summary>
    public string? DisplayName => User?.FindFirst(ClaimTypes.Name)?.Value;
}
