namespace HealthSync.Application.DTOs;

public class WorkoutLogDto
{
    public int WorkoutLogId { get; set; }
    public int UserId { get; set; }
    public DateTime WorkoutDate { get; set; }
    public int DurationMin { get; set; }
    public string? Notes { get; set; }
    public List<ExerciseSessionDto> ExerciseSessions { get; set; } = new List<ExerciseSessionDto>();
}

public class CreateWorkoutLogDto
{
    public DateTime WorkoutDate { get; set; }
    public int DurationMin { get; set; }
    public string? Notes { get; set; }
    public List<CreateExerciseSessionDto> ExerciseSessions { get; set; } = new List<CreateExerciseSessionDto>();
}

public class ExerciseSessionDto
{
    public int ExerciseSessionId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int? RestSec { get; set; }
    public decimal? Rpe { get; set; }
}

public class CreateExerciseSessionDto
{
    public int ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int? RestSec { get; set; }
    public decimal? Rpe { get; set; }
}