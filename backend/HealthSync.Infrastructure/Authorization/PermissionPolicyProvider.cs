using HealthSync.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HealthSync.Infrastructure.Authorization;

/// <summary>
/// Provider for configuring authorization policies based on permissions
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if policy name matches a known policy constant
        if (IsKnownPolicy(policyName))
        {
            var policy = BuildPolicyForName(policyName);
            if (policy != null)
            {
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        // Check if policy name matches a permission code
        if (IsPermissionCode(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fallback to default provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    private static bool IsKnownPolicy(string policyName)
    {
        // Check against Policies constants
        var policyFields = typeof(Policies).GetFields();
        return policyFields.Any(f => f.GetValue(null)?.ToString() == policyName);
    }

    private static bool IsPermissionCode(string policyName)
    {
        // Check against PermissionCodes constants
        var permissionFields = typeof(PermissionCodes).GetFields();
        return permissionFields.Any(f => f.GetValue(null)?.ToString() == policyName);
    }

    private static AuthorizationPolicy? BuildPolicyForName(string policyName)
    {
        return policyName switch
        {
            // Role-based policies
            Policies.ADMIN_ONLY => new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.ADMIN)
                .Build(),

            Policies.CUSTOMER_ONLY => new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.CUSTOMER)
                .Build(),

            Policies.ADMIN_OR_CUSTOMER => new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.ADMIN, RoleNames.CUSTOMER)
                .Build(),

            // User Management Policies
            Policies.CAN_VIEW_USERS => BuildPermissionPolicy(PermissionCodes.USER_READ),
            Policies.CAN_BAN_USERS => BuildPermissionPolicy(PermissionCodes.USER_BAN),
            Policies.CAN_UPDATE_USER_ROLES => BuildPermissionPolicy(PermissionCodes.USER_UPDATE_ROLE),
            Policies.CAN_DELETE_USERS => BuildPermissionPolicy(PermissionCodes.USER_DELETE),
            Policies.CAN_MANAGE_USERS => BuildPermissionPolicy(
                PermissionCodes.USER_READ,
                PermissionCodes.USER_UPDATE_ROLE,
                PermissionCodes.USER_DELETE),

            // Exercise Management Policies
            Policies.CAN_VIEW_EXERCISES => BuildPermissionPolicy(PermissionCodes.EXERCISE_READ),
            Policies.CAN_CREATE_EXERCISES => BuildPermissionPolicy(PermissionCodes.EXERCISE_CREATE),
            Policies.CAN_UPDATE_EXERCISES => BuildPermissionPolicy(PermissionCodes.EXERCISE_UPDATE),
            Policies.CAN_DELETE_EXERCISES => BuildPermissionPolicy(PermissionCodes.EXERCISE_DELETE),
            Policies.CAN_MANAGE_EXERCISES => BuildPermissionPolicy(
                PermissionCodes.EXERCISE_CREATE,
                PermissionCodes.EXERCISE_UPDATE,
                PermissionCodes.EXERCISE_DELETE),

            // Food Management Policies
            Policies.CAN_VIEW_FOODS => BuildPermissionPolicy(PermissionCodes.FOOD_READ),
            Policies.CAN_CREATE_FOODS => BuildPermissionPolicy(PermissionCodes.FOOD_CREATE),
            Policies.CAN_UPDATE_FOODS => BuildPermissionPolicy(PermissionCodes.FOOD_UPDATE),
            Policies.CAN_DELETE_FOODS => BuildPermissionPolicy(PermissionCodes.FOOD_DELETE),
            Policies.CAN_MANAGE_FOODS => BuildPermissionPolicy(
                PermissionCodes.FOOD_CREATE,
                PermissionCodes.FOOD_UPDATE,
                PermissionCodes.FOOD_DELETE),

            // Workout Log Policies
            Policies.CAN_VIEW_OWN_WORKOUT_LOGS => BuildPermissionPolicy(PermissionCodes.WORKOUT_LOG_READ),
            Policies.CAN_CREATE_OWN_WORKOUT_LOGS => BuildPermissionPolicy(PermissionCodes.WORKOUT_LOG_CREATE),
            Policies.CAN_UPDATE_OWN_WORKOUT_LOGS => BuildPermissionPolicy(PermissionCodes.WORKOUT_LOG_UPDATE),
            Policies.CAN_DELETE_OWN_WORKOUT_LOGS => BuildPermissionPolicy(PermissionCodes.WORKOUT_LOG_DELETE),
            Policies.CAN_MANAGE_OWN_WORKOUT_LOGS => BuildPermissionPolicy(
                PermissionCodes.WORKOUT_LOG_CREATE,
                PermissionCodes.WORKOUT_LOG_UPDATE,
                PermissionCodes.WORKOUT_LOG_DELETE),
            Policies.CAN_VIEW_ALL_WORKOUT_LOGS => BuildPermissionPolicy(PermissionCodes.WORKOUT_LOG_READ),

            // Nutrition Log Policies
            Policies.CAN_VIEW_OWN_NUTRITION_LOGS => BuildPermissionPolicy(PermissionCodes.NUTRITION_LOG_READ),
            Policies.CAN_CREATE_OWN_NUTRITION_LOGS => BuildPermissionPolicy(PermissionCodes.NUTRITION_LOG_CREATE),
            Policies.CAN_UPDATE_OWN_NUTRITION_LOGS => BuildPermissionPolicy(PermissionCodes.NUTRITION_LOG_UPDATE),
            Policies.CAN_DELETE_OWN_NUTRITION_LOGS => BuildPermissionPolicy(PermissionCodes.NUTRITION_LOG_DELETE),
            Policies.CAN_MANAGE_OWN_NUTRITION_LOGS => BuildPermissionPolicy(
                PermissionCodes.NUTRITION_LOG_CREATE,
                PermissionCodes.NUTRITION_LOG_UPDATE,
                PermissionCodes.NUTRITION_LOG_DELETE),
            Policies.CAN_VIEW_ALL_NUTRITION_LOGS => BuildPermissionPolicy(PermissionCodes.NUTRITION_LOG_READ),

            // Goal Policies
            Policies.CAN_VIEW_OWN_GOALS => BuildPermissionPolicy(PermissionCodes.GOAL_READ),
            Policies.CAN_CREATE_OWN_GOALS => BuildPermissionPolicy(PermissionCodes.GOAL_CREATE),
            Policies.CAN_UPDATE_OWN_GOALS => BuildPermissionPolicy(PermissionCodes.GOAL_UPDATE),
            Policies.CAN_DELETE_OWN_GOALS => BuildPermissionPolicy(PermissionCodes.GOAL_DELETE),
            Policies.CAN_MANAGE_OWN_GOALS => BuildPermissionPolicy(
                PermissionCodes.GOAL_CREATE,
                PermissionCodes.GOAL_UPDATE,
                PermissionCodes.GOAL_DELETE),
            Policies.CAN_VIEW_ALL_GOALS => BuildPermissionPolicy(PermissionCodes.GOAL_READ),

            // Dashboard Policies
            Policies.CAN_VIEW_ADMIN_DASHBOARD => BuildPermissionPolicy(PermissionCodes.DASHBOARD_ADMIN),
            Policies.CAN_VIEW_REPORTS => BuildPermissionPolicy(PermissionCodes.DASHBOARD_VIEW),

            _ => null
        };
    }

    private static AuthorizationPolicy BuildPermissionPolicy(params string[] permissionCodes)
    {
        var policyBuilder = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser();

        foreach (var permissionCode in permissionCodes)
        {
            policyBuilder.AddRequirements(new PermissionRequirement(permissionCode));
        }

        return policyBuilder.Build();
    }
}
