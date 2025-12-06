using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<string>>
{
    private readonly IApplicationDbContext _context;

    public GetUserRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .Select(ur => ur.Role.RoleName)
            .ToListAsync(cancellationToken);

        return roles;
    }
}
