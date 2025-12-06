using MediatR;

namespace HealthSync.Application.Queries;

/// <summary>
/// Query to get all permissions for a specific user
/// </summary>
public class GetUserPermissionsQuery : IRequest<List<string>>
{
    public int UserId { get; set; }
}
