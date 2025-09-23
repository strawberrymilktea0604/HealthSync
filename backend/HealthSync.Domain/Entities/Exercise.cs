namespace HealthSync.Domain.Entities;

public class Exercise
{
    public Guid ExerciseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner";
    public string? Equipment { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public ICollection<ExerciseSession> ExerciseSessions { get; set; } = new List<ExerciseSession>();
}