using System.ComponentModel.DataAnnotations;

namespace DanmaobTisax.Application.Identity;

/// <summary>
/// Configuration for JWT token authentication settings.
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// The issuer identifier for the JWT tokens.
    /// </summary>
    [Required]
    public string Issuer { get; set; } = default!;

    /// <summary>
    /// The intended audience of the JWT tokens.
    /// </summary>
    [Required]
    public string Audience { get; set; } = default!;

    /// <summary>
    /// The symmetric signing key for generating and validating tokens.
    /// Store this securely in configuration (not code).
    /// </summary>
    [Required]
    public string SigningKey { get; set; } = default!;

    /// <summary>
    /// Lifetime of access tokens in minutes.
    /// </summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    /// <summary>
    /// Lifetime of refresh tokens in days.
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 7;
}
