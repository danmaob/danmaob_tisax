using System.Security.Claims;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Identity;

public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public void UserWithMatchingPermissionClaim_Succeeds()
    {
        // Arrange
        var permissionCode = "read:users";
        var requirement = new PermissionRequirement(permissionCode);
        
        var permClaim = new Claim("perm", permissionCode);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { permClaim }, "test"));

        var handler = new PermissionAuthorizationHandler();
        var context = new AuthorizationHandlerContext(
            requirements: new[] { requirement },
            user: user,
            resource: null);

        // Act
        handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void UserWithoutAnyPermissionClaims_Fails()
    {
        // Arrange
        var permissionCode = "read:users";
        var requirement = new PermissionRequirement(permissionCode);

        var identity = new ClaimsIdentity(new[] {
            new Claim("sub", "user123"),
            new Claim("name", "John Doe")
        }, "test");
        var user = new ClaimsPrincipal(identity);

        var handler = new PermissionAuthorizationHandler();
        var context = new AuthorizationHandlerContext(
            requirements: new[] { requirement },
            user: user,
            resource: null);

        // Act
        handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void UserWithDifferentPermissionClaim_Fails()
    {
        // Arrange
        var permissionCode = "read:users";
        var requirement = new PermissionRequirement(permissionCode);

        var permClaim = new Claim("perm", "write:settings"); // Different code
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { permClaim }, "test"));

        var handler = new PermissionAuthorizationHandler();
        var context = new AuthorizationHandlerContext(
            requirements: new[] { requirement },
            user: user,
            resource: null);

        // Act
        handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void UserWithMultiplePermissionClaimsIncludingTheRequiredOne_Succeeds()
    {
        // Arrange
        var permissionCode = "read:users";
        var requirement = new PermissionRequirement(permissionCode);

        var permClaims = new[] {
            new Claim("perm", "write:settings"),
            new Claim("perm", "read:profile"),
            new Claim("perm", permissionCode) // Matching claim
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(permClaims, "test"));

        var handler = new PermissionAuthorizationHandler();
        var context = new AuthorizationHandlerContext(
            requirements: new[] { requirement },
            user: user,
            resource: null);

        // Act
        handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }
}
