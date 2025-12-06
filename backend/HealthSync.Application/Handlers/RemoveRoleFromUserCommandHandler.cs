using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveRoleFromUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var userRole = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userRole == null)
            return false; // Assignment doesn't exist

        _context.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
