namespace HealthSync.Domain.Entities;

/// <summary>
/// Represents a granular permission in the system (e.g., EXERCISE_CREATE, USER_BAN)
/// </summary>
public class Permission
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique code used for permission checking in code (e.g., "EXERCISE_CREATE", "USER_READ")
    /// </summary>
    public string PermissionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Category/Module grouping (e.g., "Exercise", "User", "Nutrition")
    /// </summary>
    public string? Category { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}