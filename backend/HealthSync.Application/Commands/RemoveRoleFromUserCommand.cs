using MediatR;

namespace HealthSync.Application.Commands;

/// <summary>
/// Command to remove a role from a user
/// </summary>
public class RemoveRoleFromUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
