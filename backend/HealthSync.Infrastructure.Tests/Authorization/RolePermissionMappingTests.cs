using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Authorization;

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
        
        // Verify all expected admin permissions are present
        Assert.Contains(PermissionCodes.USER_READ, permissions);
        Assert.Contains(PermissionCodes.USER_BAN, permissions);
        Assert.Contains(PermissionCodes.USER_UPDATE_ROLE, permissions);
        Assert.Contains(PermissionCodes.USER_DELETE, permissions);
        
        Assert.Contains(PermissionCodes.EXERCISE_READ, permissions);
        Assert.Contains(PermissionCodes.EXERCISE_CREATE, permissions);
        Assert.Contains(PermissionCodes.EXERCISE_UPDATE, permissions);
        Assert.Contains(PermissionCodes.EXERCISE_DELETE, permissions);
        
        Assert.Contains(PermissionCodes.FOOD_READ, permissions);
        Assert.Contains(PermissionCodes.FOOD_CREATE, permissions);
        Assert.Contains(PermissionCodes.FOOD_UPDATE, permissions);
        Assert.Contains(PermissionCodes.FOOD_DELETE, permissions);
        
        Assert.Contains(PermissionCodes.WORKOUT_LOG_READ, permissions);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_READ, permissions);
        Assert.Contains(PermissionCodes.GOAL_READ, permissions);
        
        Assert.Contains(PermissionCodes.DASHBOARD_VIEW, permissions);
        Assert.Contains(PermissionCodes.DASHBOARD_ADMIN, permissions);
    }

    [Fact]
    public void GetCustomerPermissions_ReturnsAllCustomerPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetCustomerPermissions();

        // Assert
        Assert.NotNull(permissions);
        Assert.NotEmpty(permissions);
        
        // Verify all expected customer permissions are present
        Assert.Contains(PermissionCodes.EXERCISE_READ, permissions);
        Assert.Contains(PermissionCodes.FOOD_READ, permissions);
        
        Assert.Contains(PermissionCodes.WORKOUT_LOG_READ, permissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_CREATE, permissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_UPDATE, permissions);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_DELETE, permissions);
        
        Assert.Contains(PermissionCodes.NUTRITION_LOG_READ, permissions);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_CREATE, permissions);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_UPDATE, permissions);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_DELETE, permissions);
        
        Assert.Contains(PermissionCodes.GOAL_READ, permissions);
        Assert.Contains(PermissionCodes.GOAL_CREATE, permissions);
        Assert.Contains(PermissionCodes.GOAL_UPDATE, permissions);
        Assert.Contains(PermissionCodes.GOAL_DELETE, permissions);
        
        Assert.Contains(PermissionCodes.DASHBOARD_VIEW, permissions);
        
        // Verify customer does NOT have admin permissions
        Assert.DoesNotContain(PermissionCodes.USER_READ, permissions);
        Assert.DoesNotContain(PermissionCodes.USER_BAN, permissions);
        Assert.DoesNotContain(PermissionCodes.DASHBOARD_ADMIN, permissions);
        Assert.DoesNotContain(PermissionCodes.EXERCISE_CREATE, permissions);
        Assert.DoesNotContain(PermissionCodes.FOOD_CREATE, permissions);
    }

    [Theory]
    [InlineData(RoleNames.ADMIN)]
    [InlineData(RoleNames.CUSTOMER)]
    public void GetPermissionsByRole_WithValidRole_ReturnsPermissions(string role)
    {
        // Act
        var permissions = RolePermissionMapping.GetPermissionsByRole(role);

        // Assert
        Assert.NotNull(permissions);
        Assert.NotEmpty(permissions);
    }

    [Fact]
    public void GetPermissionsByRole_WithAdminRole_ReturnsAdminPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetPermissionsByRole(RoleNames.ADMIN);

        // Assert
        var adminPermissions = RolePermissionMapping.GetAdminPermissions();
        Assert.Equal(adminPermissions, permissions);
    }

    [Fact]
    public void GetPermissionsByRole_WithCustomerRole_ReturnsCustomerPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetPermissionsByRole(RoleNames.CUSTOMER);

        // Assert
        var customerPermissions = RolePermissionMapping.GetCustomerPermissions();
        Assert.Equal(customerPermissions, permissions);
    }

    [Fact]
    public void GetPermissionsByRole_WithInvalidRole_ReturnsEmptyArray()
    {
        // Act
        var permissions = RolePermissionMapping.GetPermissionsByRole("InvalidRole");

        // Assert
        Assert.NotNull(permissions);
        Assert.Empty(permissions);
    }

    [Fact]
    public void GetPermissionsByRole_WithNullRole_ReturnsEmptyArray()
    {
        // Act
        var permissions = RolePermissionMapping.GetPermissionsByRole(null!);

        // Assert
        Assert.NotNull(permissions);
        Assert.Empty(permissions);
    }

    [Theory]
    [InlineData(RoleNames.ADMIN, PermissionCodes.USER_READ, true)]
    [InlineData(RoleNames.ADMIN, PermissionCodes.USER_BAN, true)]
    [InlineData(RoleNames.ADMIN, PermissionCodes.DASHBOARD_ADMIN, true)]
    [InlineData(RoleNames.CUSTOMER, PermissionCodes.WORKOUT_LOG_CREATE, true)]
    [InlineData(RoleNames.CUSTOMER, PermissionCodes.NUTRITION_LOG_CREATE, true)]
    [InlineData(RoleNames.CUSTOMER, PermissionCodes.GOAL_CREATE, true)]
    [InlineData(RoleNames.CUSTOMER, PermissionCodes.USER_READ, false)]
    [InlineData(RoleNames.CUSTOMER, PermissionCodes.USER_BAN, false)]
    [InlineData(RoleNames.CUSTOMER, PermissionCodes.DASHBOARD_ADMIN, false)]
    [InlineData("InvalidRole", PermissionCodes.USER_READ, false)]
    public void RoleHasPermission_ReturnsCorrectResult(string role, string permission, bool expected)
    {
        // Act
        var result = RolePermissionMapping.RoleHasPermission(role, permission);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRolesWithPermission_WithAdminOnlyPermission_ReturnsAdminOnly()
    {
        // Act
        var roles = RolePermissionMapping.GetRolesWithPermission(PermissionCodes.USER_READ);

        // Assert
        Assert.NotNull(roles);
        Assert.Contains(RoleNames.ADMIN, roles);
        Assert.DoesNotContain(RoleNames.CUSTOMER, roles);
    }

    [Fact]
    public void GetRolesWithPermission_WithSharedPermission_ReturnsBothRoles()
    {
        // Act
        var roles = RolePermissionMapping.GetRolesWithPermission(PermissionCodes.EXERCISE_READ);

        // Assert
        Assert.NotNull(roles);
        Assert.Contains(RoleNames.ADMIN, roles);
        Assert.Contains(RoleNames.CUSTOMER, roles);
        Assert.Equal(2, roles.Count);
    }

    [Fact]
    public void GetRolesWithPermission_WithCustomerOnlyPermission_ReturnsCorrectRoles()
    {
        // WORKOUT_LOG_CREATE is available to both Admin and Customer
        // Act
        var roles = RolePermissionMapping.GetRolesWithPermission(PermissionCodes.WORKOUT_LOG_CREATE);

        // Assert
        Assert.NotNull(roles);
        Assert.Contains(RoleNames.ADMIN, roles);
        Assert.Contains(RoleNames.CUSTOMER, roles);
    }

    [Fact]
    public void GetRolesWithPermission_WithNonExistentPermission_ReturnsEmptyList()
    {
        // Act
        var roles = RolePermissionMapping.GetRolesWithPermission("NON_EXISTENT_PERMISSION");

        // Assert
        Assert.NotNull(roles);
        Assert.Empty(roles);
    }

    [Fact]
    public void GetRolesWithPermission_WithDashboardView_ReturnsBothRoles()
    {
        // Act
        var roles = RolePermissionMapping.GetRolesWithPermission(PermissionCodes.DASHBOARD_VIEW);

        // Assert
        Assert.NotNull(roles);
        Assert.Contains(RoleNames.ADMIN, roles);
        Assert.Contains(RoleNames.CUSTOMER, roles);
    }

    [Fact]
    public void GetRolesWithPermission_WithDashboardAdmin_ReturnsAdminOnly()
    {
        // Act
        var roles = RolePermissionMapping.GetRolesWithPermission(PermissionCodes.DASHBOARD_ADMIN);

        // Assert
        Assert.NotNull(roles);
        Assert.Contains(RoleNames.ADMIN, roles);
        Assert.DoesNotContain(RoleNames.CUSTOMER, roles);
        Assert.Single(roles);
    }

    [Fact]
    public void AdminPermissions_ContainAllUserManagementPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetAdminPermissions();

        // Assert
        Assert.Contains(PermissionCodes.USER_READ, permissions);
        Assert.Contains(PermissionCodes.USER_BAN, permissions);
        Assert.Contains(PermissionCodes.USER_UPDATE_ROLE, permissions);
        Assert.Contains(PermissionCodes.USER_DELETE, permissions);
    }

    [Fact]
    public void CustomerPermissions_DoNotContainUserManagementPermissions()
    {
        // Act
        var permissions = RolePermissionMapping.GetCustomerPermissions();

        // Assert
        Assert.DoesNotContain(PermissionCodes.USER_READ, permissions);
        Assert.DoesNotContain(PermissionCodes.USER_BAN, permissions);
        Assert.DoesNotContain(PermissionCodes.USER_UPDATE_ROLE, permissions);
        Assert.DoesNotContain(PermissionCodes.USER_DELETE, permissions);
    }
}
