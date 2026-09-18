namespace DanmaobTisax.Domain.Identity;

using DanmaobTisax.Domain.Common;

/// <summary>
/// Represents a refresh token for identity authentication.
/// </summary>
public class RefreshToken : BaseEntity, ITenantOwned
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    /// <summary>Required by EF Core for materialization.</summary>
    protected RefreshToken()
    {
    }

    public RefreshToken(Guid tenantId, Guid userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        }

        TenantId = tenantId;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes this refresh token.
    /// </summary>
    /// <param name="replacedByTokenHash">Optional hash of the replacement token.</param>
    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;

        if (replacedByTokenHash is not null)
        {
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }

    /// <summary>
    /// Determines if this refresh token is active.
    /// </summary>
    public bool IsActive()
    {
        return !RevokedAtUtc.HasValue && ExpiresAtUtc > DateTime.UtcNow;
    }
}
