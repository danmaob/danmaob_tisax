using DanmaobTisax.Application.Identity;
using Microsoft.Extensions.Options;
using System;

namespace DanmaobTisax.Infrastructure.Identity;

public class PasswordPolicyValidator : IPasswordPolicyValidator
{
    private readonly PasswordPolicyOptions _options;

    public PasswordPolicyValidator(IOptions<PasswordPolicyOptions> options)
    {
        _options = options.Value;
    }

    public PasswordPolicyValidationResult Validate(string plainTextPassword)
    {
        var violatedRules = new List<string>();

        if (string.IsNullOrEmpty(plainTextPassword) || plainTextPassword.Length < _options.MinimumLength)
        {
            violatedRules.Add("MinimumLength");
        }

        if (_options.RequireUppercase && !plainTextPassword.Any(c => char.IsUpper(c)))
        {
            violatedRules.Add("RequireUppercase");
        }

        if (_options.RequireLowercase && !plainTextPassword.Any(c => char.IsLower(c)))
        {
            violatedRules.Add("RequireLowercase");
        }

        if (_options.RequireDigit && !plainTextPassword.Any(char.IsDigit))
        {
            violatedRules.Add("RequireDigit");
        }

        if (_options.RequireSpecialCharacter && !plainTextPassword.Any(c => !char.IsLetterOrDigit(c)))
        {
            violatedRules.Add("RequireSpecialCharacter");
        }

        return new PasswordPolicyValidationResult
        {
            IsValid = violatedRules.Count == 0,
            ViolatedRules = violatedRules
        };
    }
}
