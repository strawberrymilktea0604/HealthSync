namespace HealthSync.Domain.Entities;

/// <summary>
/// Many-to-Many relationship between Roles and Permissions
/// Defines which permissions are granted to which roles
/// </summary>
public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}