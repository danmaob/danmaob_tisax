namespace DanmaobTisax.Application.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SigningKey { get; set; } = null!;
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 7;
}

public sealed class AccessTokenResult
{
    public required string Token { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}

public sealed class GeneratedRefreshToken
{
    public required string RawToken { get; init; }
    public required string TokenHash { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(Guid userId, Guid tenantId, string email, IReadOnlyList<string> roleNames, IReadOnlyList<string> permissionCodes);
    GeneratedRefreshToken GenerateRefreshToken();
    string HashToken(string rawToken);
}
