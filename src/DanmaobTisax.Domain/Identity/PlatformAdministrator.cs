using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Common;

namespace DanmaobTisax.Domain.Identity;

public class PlatformAdministrator : BaseEntity, IAuditable
{
    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public int FailedLoginAttemptCount { get; private set; }

    public DateTime? LockoutEndUtc { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public DateTime PasswordChangedAtUtc { get; private set; }

    protected PlatformAdministrator()
    {
    }

    public PlatformAdministrator(string email, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (email.Length > 256)
        {
            throw new ArgumentException("Email must not exceed 256 characters.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (fullName.Length > 200)
        {
            throw new ArgumentException("Full name must not exceed 200 characters.", nameof(fullName));
        }

        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        PasswordChangedAtUtc = DateTime.UtcNow;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
        FailedLoginAttemptCount = 0;
        LockoutEndUtc = null;
    }

    public bool RecordFailedLoginAttempt(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttemptCount = FailedLoginAttemptCount + 1;

        if (FailedLoginAttemptCount >= maxAttempts)
        {
            LockoutEndUtc = DateTime.UtcNow + lockoutDuration;
            return true;
        }

        return false;
    }

    public bool IsLockedOut()
    {
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
        PasswordChangedAtUtc = DateTime.UtcNow;
    }
}
