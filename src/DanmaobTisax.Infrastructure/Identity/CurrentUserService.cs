using DanmaobTisax.Domain.Interfaces;
using System.Security.Claims;

namespace DanmaobTisax.Infrastructure.Identity;

/// <summary>
/// Implements ICurrentUserService using IHttpContextAccessor for HTTP context awareness.
/// This class is registered in the Infrastructure project's DI container.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user;

    /// <summary>
    /// Initializes a new instance with HTTP context accessor.
    /// </summary>
    public CurrentUserService(ClaimsPrincipal? user)
    {
        _user = user;
    }

    /// <summary>
    /// Gets the current user's ID if authenticated, otherwise null.
    /// </summary>
    public Guid? UserId => _user?.FindFirst(x => x.Type == "sub")?.Value != Guid.Empty.ToString()
        ? new Guid(_user!.FindFirst(x => x.Type == "sub")!.Value)
        : null;

    /// <summary>
    /// Gets the current user's display name if authenticated.
    /// </summary>
    public string? DisplayName => _user?.FindFirst(ClaimTypes.Name)?.Value;
}
