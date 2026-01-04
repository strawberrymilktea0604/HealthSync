namespace HealthSync.Domain.Entities;

/// <summary>
/// Represents a user action log for Data Warehouse & AI Context
/// </summary>
public class UserActionLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ActionType { get; set; } = string.Empty; // Login, CreateWorkout, UpdateGoal, ChatWithAI...
    public string Description { get; set; } = string.Empty; // "User added Bench Press 50kg"
    public string? MetaDataJson { get; set; } // Lưu JSON param nếu cần chi tiết
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
