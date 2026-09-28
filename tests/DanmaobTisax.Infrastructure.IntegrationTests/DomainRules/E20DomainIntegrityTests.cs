using System.Reflection;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Domain.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.DomainRules;

public class E20DomainIntegrityTests
{
    [Fact]
    public void Tenant_NameLongerThan200Characters_IsRejected()
    {
        var name = new string('a', 201);
        var exception = Assert.Throws<ArgumentException>(() => new Tenant(name));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void E20Entities_HaveNoPublicSetters()
    {
        var types = new[] { typeof(Tenant), typeof(TenantModule), typeof(Plan), typeof(PlanModule), typeof(FunctionalModule), typeof(PlatformAdministrator) };
        var violations = new List<string>();

        foreach (var type in types)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (type == typeof(TenantModule) && property.Name == "TenantId")
                {
                    continue;
                }

                if (property.SetMethod is not null && property.SetMethod.IsPublic == true)
                {
                    violations.Add(type.Name + "." + property.Name);
                }
            }
        }

        Assert.Empty(violations);
    }
}
