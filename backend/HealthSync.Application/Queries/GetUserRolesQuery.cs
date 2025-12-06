using MediatR;

namespace HealthSync.Application.Queries;

/// <summary>
/// Query to get all roles assigned to a user
/// </summary>
public class GetUserRolesQuery : IRequest<List<string>>
{
    public int UserId { get; set; }
}
