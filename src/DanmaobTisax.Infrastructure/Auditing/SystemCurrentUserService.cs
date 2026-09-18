using DanmaobTisax.Application.Interfaces;

namespace DanmaobTisax.Infrastructure.Auditing;

/// <summary>
/// Implementation of ICurrentUserService as a placeholder for system operations.
/// 
/// This service is used when no authenticated user is present (e.g., background jobs,
/// scheduled tasks, or API calls without authentication). It returns:
/// - UserId: null (no authenticated user)
/// - DisplayName: "system" (literal string representing the system context)
/// 
/// This placeholder will be replaced in subtask 13 once real authentication exists.
/// </summary>
public class SystemCurrentUserService : ICurrentUserService
{
    /// <inheritdoc/>
    public Guid? UserId => null;

    /// <inheritdoc/>
    public string? DisplayName => "system";
}