using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateExerciseCommand : IRequest<bool>
{
    public int ExerciseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? Equipment { get; set; }
    public string? Description { get; set; }
}
