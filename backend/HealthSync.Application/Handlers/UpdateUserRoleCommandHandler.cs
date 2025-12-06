using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, AdminUserDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUserDto> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new Exception($"User with ID {request.UserId} not found");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role, cancellationToken);
        if (role == null)
        {
            throw new Exception($"Role '{request.Role}' not found");
        }

        // Remove existing UserRole
        var existingUserRole = user.UserRoles.FirstOrDefault();
        if (existingUserRole != null)
        {
            _context.Remove(existingUserRole);
        }

        // Add new UserRole
        var newUserRole = new UserRole { UserId = user.UserId, RoleId = role.Id };
        _context.Add(newUserRole);

        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? "",
            Role = role.RoleName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.Profile?.AvatarUrl
        };
    }
}
