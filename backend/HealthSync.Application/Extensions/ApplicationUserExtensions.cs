using HealthSync.Domain.Entities;

namespace HealthSync.Application.Extensions;

public static class ApplicationUserExtensions
{
    public static string GetRoleName(this ApplicationUser user)
    {
        return user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";
    }

    public static bool HasPermission(this ApplicationUser user, string permissionCode)
    {
        return user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.PermissionCode == permissionCode);
    }
}