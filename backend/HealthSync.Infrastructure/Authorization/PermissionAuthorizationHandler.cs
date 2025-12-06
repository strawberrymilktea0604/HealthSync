using Microsoft.AspNetCore.Authorization;

namespace HealthSync.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if user has required permission
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        // Get all permission claims from the user
        var permissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        // Check if user has the required permission
        if (permissions.Contains(requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
