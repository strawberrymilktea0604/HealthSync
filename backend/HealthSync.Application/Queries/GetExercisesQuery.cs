using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetExercisesQuery : IRequest<List<ExerciseDto>>
{
    public string? MuscleGroup { get; set; }
    public string? Difficulty { get; set; }
    public string? Search { get; set; }
}