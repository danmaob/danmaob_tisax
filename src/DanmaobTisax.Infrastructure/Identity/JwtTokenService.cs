using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using DanmaobTisax.Application.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DanmaobTisax.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AccessTokenResult GenerateAccessToken(Guid userId, Guid tenantId, string email, IReadOnlyList<string> roleNames, IReadOnlyList<string> permissionCodes)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("sub", userId.ToString()),
            new Claim("tenant", tenantId.ToString()),
            new Claim("email", email)
        };

        foreach (var roleName in roleNames)
        {
            claims.Add(new Claim("role", roleName));
        }

        foreach (var permissionCode in permissionCodes)
        {
            claims.Add(new Claim("perm", permissionCode));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials
        );

        return new AccessTokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public GeneratedRefreshToken GenerateRefreshToken()
    {
        var rawBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(rawBytes);
        var tokenHash = HashToken(rawToken);

        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddDays(_options.RefreshTokenLifetimeDays);

        return new GeneratedRefreshToken
        {
            RawToken = rawToken,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public string HashToken(string rawToken)
    {
        var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawToken));
        sha256.Dispose();
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
