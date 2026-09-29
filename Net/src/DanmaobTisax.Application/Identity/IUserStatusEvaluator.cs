namespace DanmaobTisax.Application.Identity;

public interface IUserStatusEvaluator
{
    /// <summary>
    /// Returns true only when a user with that id exists in the current tenant AND is inactive. Returns false when the user is active and also returns false when no user row exists.
    /// </summary>
    Task<bool> IsUserBlockedAsync(Guid userId, CancellationToken cancellationToken);
}
