using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class CreateWorkoutLogCommand : IRequest<int>
{
    public int UserId { get; set; }
    public CreateWorkoutLogDto WorkoutLog { get; set; } = null!;
}