using System.Reflection;
using DanmaobTisax.Application;
using DanmaobTisax.Domain.Common;
using DanmaobTisax.Infrastructure;
using NetArchTest.Rules;
using Xunit;

namespace DanmaobTisax.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(DomainAssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfrastructureAssemblyMarker).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Any_Other_Layer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "DanmaobTisax.Application",
                "DanmaobTisax.Infrastructure",
                "DanmaobTisax.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, DescribeFailures(result));
    }

    [Fact]
    public void Application_Should_Only_Depend_On_Domain()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "DanmaobTisax.Infrastructure",
                "DanmaobTisax.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, DescribeFailures(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("DanmaobTisax.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, DescribeFailures(result));
    }

    [Fact]
    public void Api_Should_Not_Be_Referenced_By_Any_Inner_Layer()
    {
        foreach (var assembly in new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly })
        {
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn("DanmaobTisax.Api")
                .GetResult();

            Assert.True(result.IsSuccessful, DescribeFailures(result));
        }
    }

    private static string DescribeFailures(TestResult result)
    {
        if (result.IsSuccessful || result.FailingTypes is null)
        {
            return string.Empty;
        }

        var names = result.FailingTypes.Select(t => t.FullName);
        return "Failing types: " + string.Join(", ", names);
    }
}
