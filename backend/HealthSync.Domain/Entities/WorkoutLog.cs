namespace HealthSync.Domain.Entities;

public class WorkoutLog
{
    public Guid WorkoutLogId { get; set; }
    public Guid UserId { get; set; }
    public DateTime WorkoutDate { get; set; }
    public int? DurationMin { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ExerciseSession> ExerciseSessions { get; set; } = new List<ExerciseSession>();
}