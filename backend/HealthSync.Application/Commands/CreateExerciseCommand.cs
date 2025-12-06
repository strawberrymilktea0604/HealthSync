using MediatR;

namespace HealthSync.Application.Commands;

public class CreateExerciseCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}