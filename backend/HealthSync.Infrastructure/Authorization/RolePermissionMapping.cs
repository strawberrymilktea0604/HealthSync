using HealthSync.Domain.Constants;

namespace HealthSync.Infrastructure.Authorization;

/// <summary>
/// Maps roles to their respective permissions
/// </summary>
public static class RolePermissionMapping
{
    /// <summary>
    /// Get all permissions for Admin role
    /// </summary>
    public static IReadOnlyList<string> GetAdminPermissions() => new List<string>
    {
        // User Management - Full Access
        PermissionCodes.USER_READ,
        PermissionCodes.USER_BAN,
        PermissionCodes.USER_UPDATE_ROLE,
        PermissionCodes.USER_DELETE,
        
        // Exercise Management - Full Access
        PermissionCodes.EXERCISE_READ,
        PermissionCodes.EXERCISE_CREATE,
        PermissionCodes.EXERCISE_UPDATE,
        PermissionCodes.EXERCISE_DELETE,
        
        // Food Management - Full Access
        PermissionCodes.FOOD_READ,
        PermissionCodes.FOOD_CREATE,
        PermissionCodes.FOOD_UPDATE,
        PermissionCodes.FOOD_DELETE,
        
        // Workout Logs - Read All
        PermissionCodes.WORKOUT_LOG_READ,
        
        // Nutrition Logs - Read All
        PermissionCodes.NUTRITION_LOG_READ,
        
        // Goals - Read All
        PermissionCodes.GOAL_READ,
        
        // Dashboard - Admin Access
        PermissionCodes.DASHBOARD_VIEW,
        PermissionCodes.DASHBOARD_ADMIN,
    };

    /// <summary>
    /// Get all permissions for Customer role
    /// </summary>
    public static IReadOnlyList<string> GetCustomerPermissions() => new List<string>
    {
        // Exercise Library - Read Only
        PermissionCodes.EXERCISE_READ,
        
        // Food Library - Read Only
        PermissionCodes.FOOD_READ,
        
        // Workout Logs - Full CRUD on Own Data
        PermissionCodes.WORKOUT_LOG_READ,
        PermissionCodes.WORKOUT_LOG_CREATE,
        PermissionCodes.WORKOUT_LOG_UPDATE,
        PermissionCodes.WORKOUT_LOG_DELETE,
        
        // Nutrition Logs - Full CRUD on Own Data
        PermissionCodes.NUTRITION_LOG_READ,
        PermissionCodes.NUTRITION_LOG_CREATE,
        PermissionCodes.NUTRITION_LOG_UPDATE,
        PermissionCodes.NUTRITION_LOG_DELETE,
        
        // Goals - Full CRUD on Own Data
        PermissionCodes.GOAL_READ,
        PermissionCodes.GOAL_CREATE,
        PermissionCodes.GOAL_UPDATE,
        PermissionCodes.GOAL_DELETE,
        
        // Dashboard - User View
        PermissionCodes.DASHBOARD_VIEW,
    };

    /// <summary>
    /// Get permissions by role name
    /// </summary>
    /// <param name="role">Role name</param>
    /// <returns>List of permission codes</returns>
    public static IReadOnlyList<string> GetPermissionsByRole(string role)
    {
        return role switch
        {
            RoleNames.ADMIN => GetAdminPermissions(),
            RoleNames.CUSTOMER => GetCustomerPermissions(),
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Check if a role has a specific permission
    /// </summary>
    /// <param name="role">Role name</param>
    /// <param name="permission">Permission code</param>
    /// <returns>True if role has permission</returns>
    public static bool RoleHasPermission(string role, string permission)
    {
        var permissions = GetPermissionsByRole(role);
        return permissions.Contains(permission);
    }

    /// <summary>
    /// Get all roles that have a specific permission
    /// </summary>
    /// <param name="permission">Permission code</param>
    /// <returns>List of role names</returns>
    public static IReadOnlyList<string> GetRolesWithPermission(string permission)
    {
        var roles = new List<string>();
        
        if (GetAdminPermissions().Contains(permission))
        {
            roles.Add(RoleNames.ADMIN);
        }
        
        if (GetCustomerPermissions().Contains(permission))
        {
            roles.Add(RoleNames.CUSTOMER);
        }
        
        return roles;
    }
}
