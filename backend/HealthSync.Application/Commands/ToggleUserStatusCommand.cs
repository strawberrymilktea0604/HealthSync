using MediatR;

namespace HealthSync.Application.Commands;

public class ToggleUserStatusCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public bool IsActive { get; set; }
    public int CurrentUserId { get; set; } // To prevent locking self
}
