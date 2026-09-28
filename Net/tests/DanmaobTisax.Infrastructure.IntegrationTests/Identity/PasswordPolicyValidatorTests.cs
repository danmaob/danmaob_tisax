using DanmaobTisax.Application.Identity;
using DanmaobTisax.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public class PasswordPolicyValidatorTests
{
    private readonly PasswordPolicyValidator _validator;

    public PasswordPolicyValidatorTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new PasswordPolicyOptions());
        _validator = new PasswordPolicyValidator(options);
    }

    [Fact]
    public void ValidPassword_PassesAllRules()
    {
        // Arrange
        string password = "Str0ng!Pass";

        // Act
        var result = _validator.Validate(password);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ViolatedRules!);
    }

    [Fact]
    public void TooShortPassword_ViolatesMinimumLength()
    {
        // Arrange - short password with all required character types present except length
        string password = "a1A!"; // Only 4 characters, but has uppercase, lowercase, digit, and special char

        // Act
        var result = _validator.Validate(password);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("MinimumLength", result.ViolatedRules!);
        Assert.Single(result.ViolatedRules!);
    }

    [Fact]
    public void PasswordWithoutUppercase_ViolatesRequireUppercase()
    {
        // Arrange - password with minimum length, lowercase, digit, special char, but no uppercase
        string password = "str0ng!pass"; // 12 chars: lowercase (s,t,r,o,n,g,p,a,s,s), digit (0), special (!), but no uppercase

        // Act
        var result = _validator.Validate(password);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("RequireUppercase", result.ViolatedRules!);
        Assert.DoesNotContain("MinimumLength", result.ViolatedRules!);
    }

    [Fact]
    public void PasswordWithoutLowercase_ViolatesRequireLowercase()
    {
        // Arrange - password with minimum length, uppercase, digit, special char, but no lowercase
        string password = "STR0NG!PASS"; // 12 chars: uppercase (S,T,R,O,N,G,P,A,S,S), digit (0), special (!), but no lowercase

        // Act
        var result = _validator.Validate(password);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("RequireLowercase", result.ViolatedRules!);
        Assert.DoesNotContain("MinimumLength", result.ViolatedRules!);
    }

    [Fact]
    public void PasswordWithoutDigit_ViolatesRequireDigit()
    {
        // Arrange - password with minimum length, uppercase, lowercase, special char, but no digit
        string password = "Strong!PassABC"; // 14 chars: uppercase (S,T,r,o,n,g,P,a,s,s,A,B,C), lowercase (r,o,n,g,p,a,s,s,A,B,C), special (!), but no digit

        // Act
        var result = _validator.Validate(password);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("RequireDigit", result.ViolatedRules!);
        Assert.DoesNotContain("MinimumLength", result.ViolatedRules!);
    }

    [Fact]
    public void PasswordWithoutSpecialCharacter_ViolatesRequireSpecialCharacter()
    {
        // Arrange - password with minimum length, uppercase, lowercase, and digit, but no special char
        string password = "Str0ngPass123"; // 13 chars: uppercase (S), lowercase (t,r,o,n,g,P,a,s,s), digits (0,1,2,3), but no special character

        // Act
        var result = _validator.Validate(password);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("RequireSpecialCharacter", result.ViolatedRules!);
        Assert.DoesNotContain("MinimumLength", result.ViolatedRules!);
    }
}
