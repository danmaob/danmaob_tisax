using DanmaobTisax.Domain.Tenants;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class TenantModuleTests
{
    [Fact]
    public void Constructor_WithValidArguments_SetsProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var moduleCode = "Probe.Module";
        var isEnabled = true;

        // Act
        var module = new TenantModule(tenantId, moduleCode, isEnabled);

        // Assert
        Assert.Equal(tenantId, module.TenantId);
        Assert.Equal(moduleCode, module.ModuleCode);
        Assert.Equal(isEnabled, module.IsEnabled);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_Throws()
    {
        // Arrange
        var moduleCode = "Probe.Module";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new TenantModule(Guid.Empty, moduleCode, true)
        );

        Assert.NotEqual(string.Empty, exception.Message);
    }

    [Fact]
    public void Constructor_WithBlankModuleCode_Throws()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var moduleCode = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new TenantModule(tenantId, moduleCode, true)
        );

        Assert.NotEqual(string.Empty, exception.Message);
    }

    [Fact]
    public void Constructor_WithModuleCodeLongerThan100_Throws()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longModuleCode = new string('a', 101);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new TenantModule(tenantId, longModuleCode, true)
        );

        Assert.NotEqual(string.Empty, exception.Message);
    }

    [Fact]
    public void Disable_EnabledModule_SetsIsEnabledFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var module = new TenantModule(tenantId, "Probe.Module", true);

        // Act
        module.Disable();

        // Assert
        Assert.False(module.IsEnabled);
    }

    [Fact]
    public void Enable_DisabledModule_SetsIsEnabledTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var module = new TenantModule(tenantId, "Probe.Module", false);

        // Act
        module.Enable();

        // Assert
        Assert.True(module.IsEnabled);
    }
}
