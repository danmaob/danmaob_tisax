using System.Reflection;
using DanmaobTisax.Domain.Identity;

namespace DanmaobTisax.Infrastructure.IntegrationTests.DomainRules;

public class UserDomainIntegrityTests
{
    [Fact]
    public void User_HasNoPublicSettersExceptTenantId()
    {
        var violations = new List<string>();
        foreach (var property in typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (property.Name == "TenantId")
            {
                continue;
            }
            if (property.SetMethod is not null && property.SetMethod.IsPublic == true)
            {
                violations.Add("User." + property.Name);
            }
        }
        Assert.Empty(violations);
    }

    [Fact]
    public void User_EmailLongerThan256Characters_IsRejected()
    {
        var email = new string('a', 257);
        var exception = Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), email, "hash", "Test User"));
        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public void User_FullNameLongerThan200Characters_IsRejected()
    {
        var fullName = new string('a', 201);
        var exception = Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), "user@example.com", "hash", fullName));
        Assert.Equal("fullName", exception.ParamName);
    }

    [Fact]
    public void User_RenameToNameLongerThan200Characters_IsRejected()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "hash", "Test User");
        var exception = Assert.Throws<ArgumentException>(() => user.Rename(new string('a', 201)));
        Assert.Equal("fullName", exception.ParamName);
        Assert.Equal("Test User", user.FullName);
    }
}
