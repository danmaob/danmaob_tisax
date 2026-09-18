namespace DanmaobTisax.Infrastructure.Identity;

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

public class AuthenticationService : IAuthenticationService
{
    private readonly DanmaobTisaxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly PasswordPolicyOptions _passwordPolicyOptions;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(
        DanmaobTisaxDbContext context,
        IPasswordHasher passwordHasher,
        IOptions<PasswordPolicyOptions> passwordPolicyOptions,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _passwordPolicyOptions = passwordPolicyOptions.Value;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> LoginAsync(
        Guid tenantId,
        string email,
        string plainTextPassword,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        // Step 1: Find the User matching both TenantId and Email exactly.
        var user = await _context.Users.FindAsync(tenantId, email, cancellationToken);

        // If not found: verify against a decoy hash to prevent timing attacks.
        if (user is null)
        {
            VerifyPasswordAgainstDecoy(plainTextPassword);
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "InvalidCredentials"
            };
        }

        // Step 3: If user.IsLockedOut() - return without verifying password.
        if (user.IsLockedOut())
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "AccountLockedOut"
            };
        }

        // Step 4: If !user.IsActive - return AccountInactive.
        if (!user.IsActive)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "AccountInactive"
            };
        }

        // Step 5: Verify the password.
        var verificationResult = _passwordHasher.Verify(user.PasswordHash, plainTextPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            // Call RecordFailedLoginAttempt and save changes.
            user.RecordFailedLoginAttempt(
                _passwordPolicyOptions.MaxFailedAccessAttempts,
                TimeSpan.FromMinutes(_passwordPolicyOptions.LockoutDurationMinutes));
            await _context.SaveChangesAsync(cancellationToken);
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "InvalidCredentials"
            };
        }

        // Handle SuccessRehashNeeded: ChangePassword with newly computed hash before continuing.
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _passwordHasher.Hash(plainTextPassword);
            user.ChangePassword(newHash);
            await _context.SaveChangesAsync(cancellationToken);
            // After rehashing, continue with normal flow (treat as Success).
        }
        // If verificationResult == PasswordVerificationResult.Success or after rehashing, continue.

        // Step 6: Call RecordSuccessfulLogin.
        user.RecordSuccessfulLogin();

        // Step 7: Get role names and permission codes.
        var (roleNames, permissionCodes) = await GetRoleNamesAndPermissionCodesAsync(user.Id, cancellationToken);

        // Step 8: Issue tokens.
        var tokenResult = await IssueTokenPairAsync(
            user,
            roleNames,
            permissionCodes,
            ipAddress,
            cancellationToken);

        // Step 9: Save changes and return result.
        await _context.SaveChangesAsync(cancellationToken);
        return new LoginResult
        {
            Succeeded = true,
            AccessToken = tokenResult.AccessToken,
            AccessTokenExpiresAtUtc = tokenResult.AccessTokenExpiresAtUtc,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpiresAtUtc = tokenResult.RefreshTokenExpiresAtUtc
        };
    }

    public async Task<LoginResult> RefreshAsync(
        string rawRefreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        // Step 1: Hash the incoming raw token.
        var tokenHash = _jwtTokenService.HashToken(rawRefreshToken);

        // Step 2: Find the matching RefreshToken by hash.
        var refreshToken = await _context.RefreshTokens.FindAsync(tokenHash, cancellationToken);

        // If not found - InvalidRefreshToken.
        if (refreshToken is null)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "InvalidRefreshToken"
            };
        }

        // Step 4: If already revoked with non-null ReplacedByTokenHash - revoke all active tokens for that user.
        if (refreshToken.ReplacedByTokenHash is not null)
        {
            var allRefreshTokens = await _context.RefreshTokens.ToListAsync();

            foreach (var token in allRefreshTokens)
            {
                if (token.UserId == refreshToken.UserId && token.IsActive())
                {
                    token.Revoke(replacedByTokenHash: refreshToken.TokenHash);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "TokenReuseDetected"
            };
        }

        // Step 5: If !token.IsActive() - InvalidRefreshToken.
        if (!refreshToken.IsActive())
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "InvalidRefreshToken"
            };
        }

        // Step 6: Load the corresponding User; if missing - InvalidRefreshToken.
        var user = await _context.Users.FindAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = "InvalidRefreshToken"
            };
        }

        // Step 7: Get role names and permission codes.
        var (roleNames, permissionCodes) = await GetRoleNamesAndPermissionCodesAsync(user.Id, cancellationToken);

        // Step 8: Issue new token pair.
        var tokenResult = await IssueTokenPairAsync(
            user,
            roleNames,
            permissionCodes,
            ipAddress,
            cancellationToken);

        // Step 9: Revoke the old token with the new token's hash.
        refreshToken.Revoke(tokenResult.RefreshTokenHash);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResult
        {
            Succeeded = true,
            AccessToken = tokenResult.AccessToken,
            AccessTokenExpiresAtUtc = tokenResult.AccessTokenExpiresAtUtc,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpiresAtUtc = tokenResult.RefreshTokenExpiresAtUtc
        };
    }

    public async Task LogoutAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken)
    {
        // Hash the incoming token.
        var tokenHash = _jwtTokenService.HashToken(rawRefreshToken);

        // Find the matching RefreshToken.
        var refreshToken = await _context.RefreshTokens.FindAsync(tokenHash, cancellationToken);

        // Call .Revoke() if found and save changes; do nothing if not found (idempotent).
        if (refreshToken is not null)
        {
            refreshToken.Revoke();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<(IReadOnlyList<string> roleNames, IReadOnlyList<string> permissionCodes)> GetRoleNamesAndPermissionCodesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Join UserRoles (filtered by UserId) with Roles for role names.
        var userRoleRelations = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        var roleIds = userRoleRelations.Select(ur => ur.RoleId).ToList();

        if (!roleIds.Any())
        {
            return (Array.Empty<string>(), Array.Empty<string>());
        }

        // Get roles by ids we collected.
        var rolesByIds = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();

        var roleNames = rolesByIds.Select(r => r.Name).ToList();

        // From those role ids, join RolePermissions then Permissions for distinct permission codes.
        var permissionIds = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync();

        // Get the actual permissions by ids.
        var permissionList = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        var permissionCodes = permissionList.Select(p => p.Code).ToList();

        return (roleNames, permissionCodes);
    }

    private async Task<(string? AccessToken, DateTime? AccessTokenExpiresAtUtc, string? RefreshToken, DateTime? RefreshTokenExpiresAtUtc, string RefreshTokenHash)> IssueTokenPairAsync(
        User user,
        IReadOnlyList<string> roleNames,
        IReadOnlyList<string> permissionCodes,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        // Generate access token.
        var accessTokenResult = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.TenantId,
            user.Email,
            roleNames,
            permissionCodes);

        // Generate refresh token.
        var generatedRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Create a new RefreshToken from the generated hash/expiry plus the tenant/user/IP.
        var refreshTokenEntity = new RefreshToken(
            user.TenantId,
            user.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            ipAddress);

        // Add it to the context.
        _context.RefreshTokens.Add(refreshTokenEntity);

        return (
            accessTokenResult.Token,
            accessTokenResult.ExpiresAtUtc,
            generatedRefreshToken.RawToken,
            generatedRefreshToken.ExpiresAtUtc,
            generatedRefreshToken.TokenHash
        );
    }

    private void VerifyPasswordAgainstDecoy(string plainTextPassword)
    {
        // Use a fixed example hash with a valid PasswordHasher<T> format for decoy verification.
        // This ensures the two failure paths take comparable time and avoid revealing whether the user exists.
        var decoyHash = "decoy_verification_hash_example_123456789";
        var _ = _passwordHasher.Verify(decoyHash, plainTextPassword);
    }
}
