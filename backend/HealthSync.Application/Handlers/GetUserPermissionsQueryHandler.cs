using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly IApplicationDbContext _context;

    public GetUserPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
