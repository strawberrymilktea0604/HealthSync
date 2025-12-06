using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleToUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user exists
        var userExists = await _context.ApplicationUsers
            .AnyAsync(u => u.UserId == request.UserId, cancellationToken);
        
        if (!userExists)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        // Check if role exists
        var roleExists = await _context.Roles
            .AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        
        if (!roleExists)
            throw new InvalidOperationException($"Role with ID {request.RoleId} not found");

        // Check if assignment already exists
        var alreadyAssigned = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        
        if (alreadyAssigned)
            return false; // Already assigned, no action needed

        // Create new assignment
        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        _context.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
