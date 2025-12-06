using MediatR;

namespace HealthSync.Application.Commands;

public class DeleteExerciseCommand : IRequest<bool>
{
    public int ExerciseId { get; set; }
}
