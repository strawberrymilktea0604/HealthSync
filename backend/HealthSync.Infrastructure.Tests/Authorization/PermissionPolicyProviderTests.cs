using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Authorization;

public class PermissionPolicyProviderTests
{
    private readonly Mock<IOptions<AuthorizationOptions>> _mockOptions;
    private readonly PermissionPolicyProvider _provider;

    public PermissionPolicyProviderTests()
    {
        _mockOptions = new Mock<IOptions<AuthorizationOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(new AuthorizationOptions());
        _provider = new PermissionPolicyProvider(_mockOptions.Object);
    }

    [Fact]
    public async Task GetPolicyAsync_WithKnownPolicy_ReturnsCorrectPolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.ADMIN_ONLY);

        // Assert
        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, r => r is Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement);
        var roleRequirement = policy.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>().Single();
        Assert.Contains(RoleNames.ADMIN, roleRequirement.AllowedRoles);
    }
    
    [Theory]
    [InlineData(Policies.CAN_VIEW_USERS, PermissionCodes.USER_READ)]
    [InlineData(Policies.CAN_BAN_USERS, PermissionCodes.USER_BAN)]
    [InlineData(Policies.CAN_UPDATE_USER_ROLES, PermissionCodes.USER_UPDATE_ROLE)]
    [InlineData(Policies.CAN_DELETE_USERS, PermissionCodes.USER_DELETE)]
    [InlineData(Policies.CAN_VIEW_EXERCISES, PermissionCodes.EXERCISE_READ)]
    [InlineData(Policies.CAN_CREATE_EXERCISES, PermissionCodes.EXERCISE_CREATE)]
    [InlineData(Policies.CAN_UPDATE_EXERCISES, PermissionCodes.EXERCISE_UPDATE)]
    [InlineData(Policies.CAN_DELETE_EXERCISES, PermissionCodes.EXERCISE_DELETE)]
    [InlineData(Policies.CAN_VIEW_FOODS, PermissionCodes.FOOD_READ)]
    [InlineData(Policies.CAN_CREATE_FOODS, PermissionCodes.FOOD_CREATE)]
    [InlineData(Policies.CAN_UPDATE_FOODS, PermissionCodes.FOOD_UPDATE)]
    [InlineData(Policies.CAN_DELETE_FOODS, PermissionCodes.FOOD_DELETE)]
    [InlineData(Policies.CAN_VIEW_OWN_WORKOUT_LOGS, PermissionCodes.WORKOUT_LOG_READ)]
    [InlineData(Policies.CAN_CREATE_OWN_WORKOUT_LOGS, PermissionCodes.WORKOUT_LOG_CREATE)]
    [InlineData(Policies.CAN_UPDATE_OWN_WORKOUT_LOGS, PermissionCodes.WORKOUT_LOG_UPDATE)]
    [InlineData(Policies.CAN_DELETE_OWN_WORKOUT_LOGS, PermissionCodes.WORKOUT_LOG_DELETE)]
    [InlineData(Policies.CAN_VIEW_ALL_WORKOUT_LOGS, PermissionCodes.WORKOUT_LOG_READ)]
    [InlineData(Policies.CAN_VIEW_OWN_NUTRITION_LOGS, PermissionCodes.NUTRITION_LOG_READ)]
    [InlineData(Policies.CAN_CREATE_OWN_NUTRITION_LOGS, PermissionCodes.NUTRITION_LOG_CREATE)]
    [InlineData(Policies.CAN_UPDATE_OWN_NUTRITION_LOGS, PermissionCodes.NUTRITION_LOG_UPDATE)]
    [InlineData(Policies.CAN_DELETE_OWN_NUTRITION_LOGS, PermissionCodes.NUTRITION_LOG_DELETE)]
    [InlineData(Policies.CAN_VIEW_ALL_NUTRITION_LOGS, PermissionCodes.NUTRITION_LOG_READ)]
    [InlineData(Policies.CAN_VIEW_OWN_GOALS, PermissionCodes.GOAL_READ)]
    [InlineData(Policies.CAN_CREATE_OWN_GOALS, PermissionCodes.GOAL_CREATE)]
    [InlineData(Policies.CAN_UPDATE_OWN_GOALS, PermissionCodes.GOAL_UPDATE)]
    [InlineData(Policies.CAN_DELETE_OWN_GOALS, PermissionCodes.GOAL_DELETE)]
    [InlineData(Policies.CAN_VIEW_ALL_GOALS, PermissionCodes.GOAL_READ)]
    [InlineData(Policies.CAN_VIEW_ADMIN_DASHBOARD, PermissionCodes.DASHBOARD_ADMIN)]
    [InlineData(Policies.CAN_VIEW_REPORTS, PermissionCodes.DASHBOARD_VIEW)]
    public async Task GetPolicyAsync_WithKnownPermissionPolicy_ReturnsPolicyWithRequirement(string policyName, string expectedPermission)
    {
        // Act
        var policy = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        var requirement = policy.Requirements.OfType<PermissionRequirement>().Single();
        Assert.Equal(expectedPermission, requirement.PermissionCode);
    }

    [Fact]
    public async Task GetPolicyAsync_WithCompositePolicy_ReturnsPolicyWithMultipleRequirements()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CAN_MANAGE_USERS);

        // Assert
        Assert.NotNull(policy);
        var requirements = policy.Requirements.OfType<PermissionRequirement>().Select(r => r.PermissionCode).ToList();
        Assert.Contains(PermissionCodes.USER_READ, requirements);
        Assert.Contains(PermissionCodes.USER_UPDATE_ROLE, requirements);
        Assert.Contains(PermissionCodes.USER_DELETE, requirements);
        Assert.Equal(3, requirements.Count);
    }

    [Fact]
    public async Task GetPolicyAsync_WithPermissionPrefix_ReturnsDynamicPolicy()
    {
        // Arrange
        var permissionCode = "Custom.Permission";
        var policyName = $"Permission:{permissionCode}";

        // Act
        var policy = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        var requirement = policy.Requirements.OfType<PermissionRequirement>().Single();
        Assert.Equal(permissionCode, requirement.PermissionCode);
    }

    [Fact]
    public async Task GetPolicyAsync_WithDirectPermissionCode_ReturnsPolicy()
    {
        // Arrange
        var permissionCode = PermissionCodes.USER_READ;

        // Act
        var policy = await _provider.GetPolicyAsync(permissionCode);

        // Assert
        Assert.NotNull(policy);
        var requirement = policy.Requirements.OfType<PermissionRequirement>().Single();
        Assert.Equal(permissionCode, requirement.PermissionCode);
    }

    [Fact]
    public async Task GetPolicyAsync_WithUnknownPolicy_ReturnsNull()
    {
        // Arrange
        var policyName = "UnknownPolicy";

        // Act
        var policy = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ReturnsFallbackDefaultPolicy()
    {
        // Act
        var policy = await _provider.GetDefaultPolicyAsync();

        // Assert
        // The default policy from DefaultAuthorizationPolicyProvider requires authenticated user
        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, r => r is Microsoft.AspNetCore.Authorization.Infrastructure.DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public async Task GetFallbackPolicyAsync_ReturnsFallbackPolicy()
    {
        // Act
        var policy = await _provider.GetFallbackPolicyAsync();

        // Assert
        // By default, fallback policy is null unless configured otherwise
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithCustomerOnlyPolicy_ReturnsCorrectPolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CUSTOMER_ONLY);

        // Assert
        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, r => r is Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement);
        var roleRequirement = policy.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>().Single();
        Assert.Contains(RoleNames.CUSTOMER, roleRequirement.AllowedRoles);
    }

    [Fact]
    public async Task GetPolicyAsync_WithAdminOrCustomerPolicy_ReturnsCorrectPolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.ADMIN_OR_CUSTOMER);

        // Assert
        Assert.NotNull(policy);
        var roleRequirement = policy.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>().Single();
        Assert.Contains(RoleNames.ADMIN, roleRequirement.AllowedRoles);
        Assert.Contains(RoleNames.CUSTOMER, roleRequirement.AllowedRoles);
    }

    [Fact]
    public async Task GetPolicyAsync_WithManageExercisesPolicy_ReturnsCompositePolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CAN_MANAGE_EXERCISES);

        // Assert
        Assert.NotNull(policy);
        var requirements = policy.Requirements.OfType<PermissionRequirement>().Select(r => r.PermissionCode).ToList();
        Assert.Contains(PermissionCodes.EXERCISE_CREATE, requirements);
        Assert.Contains(PermissionCodes.EXERCISE_UPDATE, requirements);
        Assert.Contains(PermissionCodes.EXERCISE_DELETE, requirements);
        Assert.Equal(3, requirements.Count);
    }

    [Fact]
    public async Task GetPolicyAsync_WithManageFoodsPolicy_ReturnsCompositePolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CAN_MANAGE_FOODS);

        // Assert
        Assert.NotNull(policy);
        var requirements = policy.Requirements.OfType<PermissionRequirement>().Select(r => r.PermissionCode).ToList();
        Assert.Contains(PermissionCodes.FOOD_CREATE, requirements);
        Assert.Contains(PermissionCodes.FOOD_UPDATE, requirements);
        Assert.Contains(PermissionCodes.FOOD_DELETE, requirements);
        Assert.Equal(3, requirements.Count);
    }

    [Fact]
    public async Task GetPolicyAsync_WithManageOwnWorkoutLogsPolicy_ReturnsCompositePolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CAN_MANAGE_OWN_WORKOUT_LOGS);

        // Assert
        Assert.NotNull(policy);
        var requirements = policy.Requirements.OfType<PermissionRequirement>().Select(r => r.PermissionCode).ToList();
        Assert.Contains(PermissionCodes.WORKOUT_LOG_CREATE, requirements);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_UPDATE, requirements);
        Assert.Contains(PermissionCodes.WORKOUT_LOG_DELETE, requirements);
        Assert.Equal(3, requirements.Count);
    }

    [Fact]
    public async Task GetPolicyAsync_WithManageOwnNutritionLogsPolicy_ReturnsCompositePolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CAN_MANAGE_OWN_NUTRITION_LOGS);

        // Assert
        Assert.NotNull(policy);
        var requirements = policy.Requirements.OfType<PermissionRequirement>().Select(r => r.PermissionCode).ToList();
        Assert.Contains(PermissionCodes.NUTRITION_LOG_CREATE, requirements);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_UPDATE, requirements);
        Assert.Contains(PermissionCodes.NUTRITION_LOG_DELETE, requirements);
        Assert.Equal(3, requirements.Count);
    }

    [Fact]
    public async Task GetPolicyAsync_WithManageOwnGoalsPolicy_ReturnsCompositePolicy()
    {
        // Act
        var policy = await _provider.GetPolicyAsync(Policies.CAN_MANAGE_OWN_GOALS);

        // Assert
        Assert.NotNull(policy);
        var requirements = policy.Requirements.OfType<PermissionRequirement>().Select(r => r.PermissionCode).ToList();
        Assert.Contains(PermissionCodes.GOAL_CREATE, requirements);
        Assert.Contains(PermissionCodes.GOAL_UPDATE, requirements);
        Assert.Contains(PermissionCodes.GOAL_DELETE, requirements);
        Assert.Equal(3, requirements.Count);
    }
}


