using Xunit;
using DanmaobTisax.Domain.Identity;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public class UserLockoutTests
{
    [Fact]
    public void FailedAttemptsBelowThreshold_DoesNotLockAccount()
    {
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "Test User";

        var user = new User(tenantId, email, passwordHash, fullName);

        var maxAttempts = 5;
        var lockoutDuration = TimeSpan.FromMinutes(15);

        // Call 1
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());

        // Call 2
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());

        // Call 3
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());
    }

    [Fact]
    public void FailedAttemptsReachingThreshold_LocksAccount()
    {
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "Test User";

        var user = new User(tenantId, email, passwordHash, fullName);

        var maxAttempts = 5;
        var lockoutDuration = TimeSpan.FromMinutes(15);

        // Call 1
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());

        // Call 2
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());

        // Call 3
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());

        // Call 4
        Assert.False(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.False(user.IsLockedOut());

        // Call 5 - reaches threshold
        Assert.True(user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration));
        Assert.True(user.IsLockedOut());
    }

    [Fact]
    public void LockedOutAccount_RemainsLockedUntilLockoutEndUtc()
    {
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "Test User";

        var user = new User(tenantId, email, passwordHash, fullName);

        var maxAttempts = 5;
        var lockoutDuration = TimeSpan.FromMilliseconds(1);

        // Accumulate failed attempts to reach threshold
        for (int i = 0; i < maxAttempts - 1; i++)
        {
            user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration);
        }

        // Make the attempt that triggers lockout
        user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration);

        Assert.True(user.IsLockedOut());

        // Wait for lockout to expire (at least 50ms)
        Thread.Sleep(100);

        Assert.False(user.IsLockedOut());
    }

    [Fact]
    public void SuccessfulLogin_ResetsFailedAttemptCountAndClearsLockout()
    {
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "Test User";

        var user = new User(tenantId, email, passwordHash, fullName);

        var maxAttempts = 5;
        var lockoutDuration = TimeSpan.FromMinutes(15);

        // Accumulate failed attempts to reach threshold
        for (int i = 0; i < maxAttempts - 1; i++)
        {
            user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration);
        }

        // Make the attempt that triggers lockout
        user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration);

        Assert.True(user.IsLockedOut());

        // Simulate successful login
        user.RecordSuccessfulLogin();

        Assert.False(user.IsLockedOut());
    }
}
