using MediatR;

namespace HealthSync.Application.Commands;

public class DeleteWorkoutLogCommand : IRequest<bool>
{
    public int WorkoutLogId { get; set; }
    public int UserId { get; set; }
}
