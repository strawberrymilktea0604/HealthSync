using Microsoft.AspNetCore.Authorization;

namespace HealthSync.Infrastructure.Authorization;

/// <summary>
/// Custom authorization requirement based on permission codes
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}
