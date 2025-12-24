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

public class RolePermissionMappingTests
{
    [Fact]
    public void GetAdminPermissions_ReturnsAllAdminPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetAdminPermissions();

        // Assert
        Assert.NotNull(permissions);
        Assert.NotEmpty(permissions);
        Assert.Contains(PermissionCodes.USER_READ, permissions);
        Assert.Contains(PermissionCodes.USER_DELETE, permissions);
        Assert.Contains(PermissionCodes.EXERCISE_CREATE, permissions);
        Assert.Contains(PermissionCodes.FOOD_CREATE, permissions);
    }

    [Fact]
    public void GetCustomerPermissions_ReturnsCustomerPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetCustomerPermissions();

        // Assert
        Assert.NotNull(permissions);
        Assert.NotEmpty(permissions);
        Assert.Contains(PermissionCodes.EXERCISE_READ, permissions);
        Assert.Contains(PermissionCodes.FOOD_READ, permissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_READ, permissions);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_READ, permissions);
        
        // Customer should NOT have delete permissions
        Assert.DoesNotContain(PermissionCodes.USER_DELETE, permissions);
        Assert.DoesNotContain(PermissionCodes.EXERCISE_DELETE, permissions);
    }

    [Fact]
    public void AdminPermissions_ContainAllCRUDOperations()
    {
        // Arrange
        var adminPermissions = RolePermissionMapping.GetAdminPermissions();

        // Assert - Admin should have full CRUD for exercises
        Assert.Contains(PermissionCodes.EXERCISE_READ, adminPermissions);
        Assert.Contains(PermissionCodes.EXERCISE_CREATE, adminPermissions);
        Assert.Contains(PermissionCodes.EXERCISE_UPDATE, adminPermissions);
        Assert.Contains(PermissionCodes.EXERCISE_DELETE, adminPermissions);

        // Admin should have full CRUD for food
        Assert.Contains(PermissionCodes.FOOD_READ, adminPermissions);
        Assert.Contains(PermissionCodes.FOOD_CREATE, adminPermissions);
        Assert.Contains(PermissionCodes.FOOD_UPDATE, adminPermissions);
        Assert.Contains(PermissionCodes.FOOD_DELETE, adminPermissions);
    }

    [Fact]
    public void CustomerPermissions_OnlyHaveReadAndOwnDataOperations()
    {
        // Arrange
        var customerPermissions = RolePermissionMapping.GetCustomerPermissions();

        // Assert - Customer can read exercises but not create/update/delete
        Assert.Contains(PermissionCodes.EXERCISE_READ, customerPermissions);
        Assert.DoesNotContain(PermissionCodes.EXERCISE_CREATE, customerPermissions);
        Assert.DoesNotContain(PermissionCodes.EXERCISE_UPDATE, customerPermissions);
        Assert.DoesNotContain(PermissionCodes.EXERCISE_DELETE, customerPermissions);

        // Customer can manage their own workout logs
        Assert.Contains(PermissionCodes.WORKOUT_LOG_READ, customerPermissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_CREATE, customerPermissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_UPDATE, customerPermissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_DELETE, customerPermissions);
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
