namespace HealthSync.Application.DTOs;

public class ExerciseDto
{
    public int ExerciseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner";
    public string? Equipment { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class CreateExerciseDto
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner";
    public string? Equipment { get; set; }
    public string? Description { get; set; }
}

public class UpdateExerciseDto
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner";
    public string? Equipment { get; set; }
    public string? Description { get; set; }
}