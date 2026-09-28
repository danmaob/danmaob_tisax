using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DanmaobTisax.Infrastructure.Identity;

public class PlatformAuthenticationService : IPlatformAuthenticationService
{
    private readonly DanmaobTisaxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly PasswordPolicyOptions _passwordPolicyOptions;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly string _decoyPasswordHash;

    public PlatformAuthenticationService(DanmaobTisaxDbContext context, IPasswordHasher passwordHasher, IOptions<PasswordPolicyOptions> passwordPolicyOptions, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _passwordPolicyOptions = passwordPolicyOptions.Value;
        _jwtTokenService = jwtTokenService;
        _decoyPasswordHash = _passwordHasher.Hash("decoy-password-does-not-match-anything-Az9!");
    }

    public async Task<LoginResult> LoginAsync(string email, string plainTextPassword, CancellationToken cancellationToken)
    {
        var administrator = await _context.PlatformAdministrators.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);

        if (administrator is null)
        {
            _ = _passwordHasher.Verify(_decoyPasswordHash, plainTextPassword);
            return new LoginResult { Succeeded = false, FailureReason = "InvalidCredentials" };
        }

        if (administrator.IsLockedOut() == true)
        {
            return new LoginResult { Succeeded = false, FailureReason = "AccountLockedOut" };
        }

        var verificationResult = _passwordHasher.Verify(administrator.PasswordHash, plainTextPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            administrator.RecordFailedLoginAttempt(_passwordPolicyOptions.MaxFailedAccessAttempts, TimeSpan.FromMinutes(_passwordPolicyOptions.LockoutDurationMinutes));
            await _context.SaveChangesAsync(cancellationToken);
            return new LoginResult { Succeeded = false, FailureReason = "InvalidCredentials" };
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            administrator.ChangePassword(_passwordHasher.Hash(plainTextPassword));
        }

        administrator.RecordSuccessfulLogin();

        var permissionCodes = await _context.Permissions.Where(p => p.Module == "Platform").Select(p => p.Module + "." + p.Action).ToListAsync(cancellationToken);

        var accessToken = _jwtTokenService.GeneratePlatformAccessToken(administrator.Id, administrator.Email, permissionCodes);

        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResult { Succeeded = true, AccessToken = accessToken.Token, AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc };
    }
}
