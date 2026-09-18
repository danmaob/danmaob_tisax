using DanmaobTisax.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Configuration;

public class RequiredConfigurationValidatorTests
{
    private const string Key = "ConnectionStrings:DefaultConnection";

    [Fact]
    public void EnsurePresent_Throws_When_Key_Is_Missing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        Assert.Throws<InvalidOperationException>(
            () => RequiredConfigurationValidator.EnsurePresent(configuration, Key, "Some guidance."));
    }

    [Fact]
    public void EnsurePresent_Does_Not_Throw_When_Key_Is_Present()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [Key] = "Server=dummy;Database=dummy;" })
            .Build();

        RequiredConfigurationValidator.EnsurePresent(configuration, Key, "Some guidance.");
    }

    [Fact]
    public void EnvironmentVariable_Takes_Precedence_Over_LowerPriorityConfiguration()
    {
        const string envVarKey = "ConnectionStrings__DefaultConnection";
        const string fromLowerPrioritySource = "Server=from-appsettings-simulated;Database=dummy;";
        const string fromEnvironment = "Server=from-environment-simulated;Database=dummy;";

        Environment.SetEnvironmentVariable(envVarKey, fromEnvironment);
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { [Key] = fromLowerPrioritySource })
                .AddEnvironmentVariables()
                .Build();

            Assert.Equal(fromEnvironment, configuration[Key]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVarKey, null);
        }
    }
}
