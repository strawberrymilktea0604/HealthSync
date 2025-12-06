using Microsoft.AspNetCore.Authorization;

namespace HealthSync.Infrastructure.Authorization;

/// <summary>
/// Custom attribute to decorate controllers/actions with permission requirements
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = $"Permission:{permissionCode}";
    }
}
