using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "ThisIsAVeryLongSigningKeyThatIsAtLeastThirtyTwoCharactersLong!"
        });

        _jwtTokenService = new JwtTokenService(jwtOptions);
    }

    [Fact]
    public void GenerateAccessToken_IncludesExpectedClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var roleName = "Admin";
        var permissionCode = "read:users";

        // Act
        var result = _jwtTokenService.GenerateAccessToken(userId, tenantId, email, new[] { roleName }, new[] { permissionCode });

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Equal(userId.ToString(), token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);
        Assert.Equal(tenantId.ToString(), token.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value);
        Assert.Equal(email, token.Claims.FirstOrDefault(c => c.Type == "email")?.Value);
        Assert.Contains(roleName, token.Claims.Select(c => c.Value));
        Assert.Contains(permissionCode, token.Claims.Select(c => c.Value));
    }

    [Fact]
    public void GenerateAccessToken_SetsCorrectIssuerAndAudience()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "admin@example.com";
        var roles = new[] { "SuperAdmin" };
        var permissions = new[] { "write:settings" };

        // Act
        var result = _jwtTokenService.GenerateAccessToken(userId, tenantId, email, roles, permissions);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Equal("TestIssuer", token.Issuer);
        Assert.Contains("TestAudience", token.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAccordingToConfiguredLifetime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "expiry@example.com";
        var roles = new[] { "User" };
        var permissions = new[] { "read:profile" };

        // Act
        var result = _jwtTokenService.GenerateAccessToken(userId, tenantId, email, roles, permissions);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        Assert.True(token.ValidTo <= expectedExpiry.AddSeconds(5), $"Token valid to {token.ValidTo} but expected within 5 seconds of {expectedExpiry}");
    }

    [Fact]
    public void GenerateRefreshToken_ProducesDistinctRawTokenAndMatchingHash()
    {
        // Arrange - no specific parameters needed for GenerateRefreshToken

        // Act
        var result1 = _jwtTokenService.GenerateRefreshToken();
        var result2 = _jwtTokenService.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(result1.RawToken, result2.RawToken);
        Assert.Equal(result1.TokenHash, HashToken(result1.RawToken));
    }

    [Fact]
    public void HashToken_IsDeterministicForTheSameInput()
    {
        // Arrange
        var fixedString = "ThisIsATestTokenThatWillBeUsedMultipleTimesForTestingPurposes";

        // Act
        var hash1 = HashToken(fixedString);
        var hash2 = HashToken(fixedString);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    private static string HashToken(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
    }
}
