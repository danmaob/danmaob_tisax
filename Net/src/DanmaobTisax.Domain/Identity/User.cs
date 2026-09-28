namespace DanmaobTisax.Domain.Identity;

using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Common;

public class User : BaseEntity, ITenantOwned, IAuditable
{
    public Guid TenantId { get; set; }
    public string Email { get; private set; } = string.Empty;
    [AuditRedacted]
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int FailedLoginAttemptCount { get; private set; } = 0;
    public DateTime? LockoutEndUtc { get; private set; } = null;
    public DateTime? LastLoginAtUtc { get; private set; } = null;
    public DateTime PasswordChangedAtUtc { get; private set; }

    /// <summary>Required by EF Core for materialization.</summary>
    protected User()
    {
    }

    public User(Guid tenantId, string email, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (email.Length > 256)
        {
            throw new ArgumentException("Email must not exceed 256 characters.", nameof(email));
        }

        if (fullName.Length > 200)
        {
            throw new ArgumentException("Full name must not exceed 200 characters.", nameof(fullName));
        }

        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        PasswordChangedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Records a successful login attempt.</summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
        FailedLoginAttemptCount = 0;
        LockoutEndUtc = null;
    }

    /// <summary>Records a failed login attempt and potentially applies lockout.</summary>
    /// <param name="maxAttempts">Maximum allowed login attempts before lockout.</param>
    /// <param name="lockoutDuration">Duration of the lockout period.</param>
    /// <returns>True if the account is now locked out, false otherwise.</returns>
    public bool RecordFailedLoginAttempt(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttemptCount++;

        if (FailedLoginAttemptCount >= maxAttempts)
        {
            LockoutEndUtc = DateTime.UtcNow + lockoutDuration;
            return true;
        }

        return false;
    }

    /// <summary>Determines whether the user is currently locked out.</summary>
    public bool IsLockedOut()
    {
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
    }

    /// <summary>Changes the user's password hash.</summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordChangedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Changes the user's full name.</summary>
    public void Rename(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (fullName.Length > 200)
        {
            throw new ArgumentException("Full name must not exceed 200 characters.", nameof(fullName));
        }

        FullName = fullName;
    }

    /// <summary>Deactivates the user.</summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>Reactivates the user.</summary>
    public void Reactivate()
    {
        IsActive = true;
    }
}
