namespace HealthSync.Domain.Entities;

public class ExerciseSession
{
    public int ExerciseSessionId { get; set; }
    public int WorkoutLogId { get; set; }
    public int ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int? RestSec { get; set; }
    public decimal? Rpe { get; set; }

    // Navigation properties
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}