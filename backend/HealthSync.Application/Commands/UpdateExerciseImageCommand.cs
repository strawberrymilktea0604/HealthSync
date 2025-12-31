using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateExerciseImageCommand : IRequest<Unit>
{
    public int ExerciseId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
