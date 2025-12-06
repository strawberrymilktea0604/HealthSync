using MediatR;

namespace HealthSync.Application.Commands;

/// <summary>
/// Command to assign a role to a user
/// </summary>
public class AssignRoleToUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
