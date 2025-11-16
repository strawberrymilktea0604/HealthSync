namespace HealthSync.Domain.Entities;

public class WorkoutLog
{
    public int WorkoutLogId { get; set; }
    public int UserId { get; set; }
    public DateTime WorkoutDate { get; set; }
    public int DurationMin { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ExerciseSession> ExerciseSessions { get; set; } = new List<ExerciseSession>();
}