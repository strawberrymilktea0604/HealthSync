using System.Security.Claims;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace HealthSync.Infrastructure.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WithMatchingPermission_Succeeds()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement(PermissionCodes.USER_READ);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("Permission", PermissionCodes.USER_READ),
            new Claim("Permission", PermissionCodes.USER_UPDATE_ROLE)
        }));

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutMatchingPermission_Fails()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement(PermissionCodes.USER_DELETE);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("Permission", PermissionCodes.USER_READ),
            new Claim("Permission", PermissionCodes.EXERCISE_READ)
        }));

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoPermissions_Fails()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement(PermissionCodes.USER_READ);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultiplePermissions_FindsCorrectOne()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement(PermissionCodes.EXERCISE_DELETE);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("Permission", PermissionCodes.USER_READ),
            new Claim("Permission", PermissionCodes.EXERCISE_READ),
            new Claim("Permission", PermissionCodes.EXERCISE_CREATE),
            new Claim("Permission", PermissionCodes.EXERCISE_UPDATE),
            new Claim("Permission", PermissionCodes.EXERCISE_DELETE)
        }));

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }
}

public class PermissionRequirementTests
{
    [Fact]
    public void Constructor_WithValidPermissionCode_CreatesInstance()
    {
        // Arrange & Act
        var requirement = new PermissionRequirement(PermissionCodes.USER_READ);

        // Assert
        Assert.Equal(PermissionCodes.USER_READ, requirement.PermissionCode);
    }

    [Fact]
    public void Constructor_WithEmptyPermissionCode_CreatesInstance()
    {
        // Arrange & Act
        var requirement = new PermissionRequirement("");

        // Assert
        Assert.Equal("", requirement.PermissionCode);
    }
}



public class RequirePermissionAttributeTests
{
    [Fact]
    public void Constructor_WithValidPermissionCode_CreatesInstance()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute(PermissionCodes.USER_READ);

        // Assert
        Assert.Equal($"Permission:{PermissionCodes.USER_READ}", attribute.Policy);
    }

    [Fact]
    public void Constructor_WithEmptyPermissionCode_CreatesInstanceWithPrefixedPolicy()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("");

        // Assert
        Assert.Equal("Permission:", attribute.Policy);
    }

    [Fact]
    public void Attribute_InheritsFromAuthorizeAttribute()
    {
        // Arrange
        var attribute = new RequirePermissionAttribute(PermissionCodes.USER_READ);

        // Assert
        Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
    }
}
