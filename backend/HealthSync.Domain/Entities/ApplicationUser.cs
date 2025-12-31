namespace HealthSync.Domain.Entities;

/// <summary>
/// Represents an application user account
/// </summary>
public class ApplicationUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public UserProfile? Profile { get; set; }
    public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
    public ICollection<NutritionLog> NutritionLogs { get; set; } = new List<NutritionLog>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}