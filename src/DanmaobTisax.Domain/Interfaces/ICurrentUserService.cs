namespace DanmaobTisax.Domain.Interfaces;

/// <summary>
/// Represents the current user from the HTTP context.
/// Used for audit logging and authorization purposes.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user, if authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the display name of the current user, if available.
    /// </summary>
    string? DisplayName { get; }
}
