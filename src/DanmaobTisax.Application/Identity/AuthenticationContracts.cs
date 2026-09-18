namespace DanmaobTisax.Application.Identity;

public sealed class LoginResult
{
    public bool Succeeded { get; init; }
    public string? FailureReason { get; init; }
    public string? AccessToken { get; init; }
    public DateTime? AccessTokenExpiresAtUtc { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? RefreshTokenExpiresAtUtc { get; init; }
}

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(Guid tenantId, string email, string plainTextPassword, string? ipAddress, CancellationToken cancellationToken);
    Task<LoginResult> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken cancellationToken);
    Task LogoutAsync(string rawRefreshToken, CancellationToken cancellationToken);
}
