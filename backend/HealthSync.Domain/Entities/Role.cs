namespace HealthSync.Domain.Entities;

/// <summary>
/// Represents a role in the system (e.g., Admin, Customer)
/// </summary>
public class Role
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique role name (e.g., "Admin", "Customer")
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this role represents
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}