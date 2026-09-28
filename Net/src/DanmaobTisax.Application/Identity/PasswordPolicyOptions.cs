namespace DanmaobTisax.Application.Identity;

public sealed class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    public int MinimumLength { get; init; } = 10;
    public bool RequireUppercase { get; init; } = true;
    public bool RequireLowercase { get; init; } = true;
    public bool RequireDigit { get; init; } = true;
    public bool RequireSpecialCharacter { get; init; } = true;
    public int MaxFailedAccessAttempts { get; init; } = 5;
    public int LockoutDurationMinutes { get; init; } = 15;
}

public interface IPasswordPolicyValidator
{
    PasswordPolicyValidationResult Validate(string plainTextPassword);
}

public sealed class PasswordPolicyValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string>? ViolatedRules { get; init; }
}
