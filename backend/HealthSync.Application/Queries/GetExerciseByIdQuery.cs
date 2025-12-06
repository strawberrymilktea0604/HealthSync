using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetExerciseByIdQuery : IRequest<ExerciseDto?>
{
    public int ExerciseId { get; set; }
}
