global using DanmaobTisax.Domain.Tenants;
global using DanmaobTisax.Domain.Exceptions;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public class TenantStatusTransitionTests
{
    [Fact]
    public void NewTenant_HasActiveStatus()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");

        // Assert
        Assert.Equal(TenantStatus.Active, tenant.Status);
    }

    [Fact]
    public void Suspend_FromActive_SetsSuspended()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");

        // Act
        tenant.Suspend();

        // Assert
        Assert.Equal(TenantStatus.Suspended, tenant.Status);
    }

    [Fact]
    public void Suspend_FromSuspended_Throws()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Suspend();

        // Act
        var exception = Assert.Throws<InvalidTenantStatusTransitionException>(() => tenant.Suspend());

        // Assert
        Assert.Equal(TenantStatus.Suspended, tenant.Status);
    }

    [Fact]
    public void Suspend_FromDeactivated_Throws()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Deactivate();

        // Act
        var exception = Assert.Throws<InvalidTenantStatusTransitionException>(() => tenant.Suspend());

        // Assert
        Assert.Equal(TenantStatus.Deactivated, tenant.Status);
    }

    [Fact]
    public void Reactivate_FromSuspended_SetsActive()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Suspend();

        // Act
        tenant.Reactivate();

        // Assert
        Assert.Equal(TenantStatus.Active, tenant.Status);
    }

    [Fact]
    public void Reactivate_FromActive_Throws()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");

        // Act
        var exception = Assert.Throws<InvalidTenantStatusTransitionException>(() => tenant.Reactivate());

        // Assert
        Assert.Equal(TenantStatus.Active, tenant.Status);
    }

    [Fact]
    public void Reactivate_FromDeactivated_Throws()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Deactivate();

        // Act
        var exception = Assert.Throws<InvalidTenantStatusTransitionException>(() => tenant.Reactivate());

        // Assert
        Assert.Equal(TenantStatus.Deactivated, tenant.Status);
    }

    [Fact]
    public void Deactivate_FromActive_SetsDeactivated()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");

        // Act
        tenant.Deactivate();

        // Assert
        Assert.Equal(TenantStatus.Deactivated, tenant.Status);
    }

    [Fact]
    public void Deactivate_FromSuspended_SetsDeactivated()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Suspend();

        // Act
        tenant.Deactivate();

        // Assert
        Assert.Equal(TenantStatus.Deactivated, tenant.Status);
    }

    [Fact]
    public void Deactivate_FromDeactivated_Throws()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Deactivate();

        // Act
        var exception = Assert.Throws<InvalidTenantStatusTransitionException>(() => tenant.Deactivate());

        // Assert
        Assert.Equal(TenantStatus.Deactivated, tenant.Status);
    }

    [Fact]
    public void Exception_ExposesCurrentStatus()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant");
        tenant.Suspend();

        // Act
        var exception = Assert.Throws<InvalidTenantStatusTransitionException>(() => tenant.Suspend());

        // Assert
        Assert.Equal(TenantStatus.Suspended, exception.CurrentStatus);
    }
}
