using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, AdminUserDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        // Update full name in profile
        if (user.Profile != null)
        {
            user.Profile.FullName = request.FullName;
        }
        else
        {
            // Create profile if not exists
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = request.FullName,
                Gender = "Unknown",
                HeightCm = 0,
                WeightKg = 0,
                Dob = DateTime.UtcNow.AddYears(-20),
                ActivityLevel = "Moderate"
            };
            _context.Add(profile);
        }

        // Update role if changed
        var currentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName;
        if (currentRole != request.Role)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role, cancellationToken);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role '{request.Role}' not found");
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
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Get updated data
        var updatedUser = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        return new AdminUserDto
        {
            UserId = updatedUser!.UserId,
            UserName = updatedUser.UserName,
            Email = updatedUser.Email,
            FullName = updatedUser.Profile?.FullName ?? string.Empty,
            Role = updatedUser.UserRoles.FirstOrDefault()?.Role?.RoleName ?? string.Empty,
            IsActive = updatedUser.IsActive,
            CreatedAt = updatedUser.CreatedAt,
            AvatarUrl = updatedUser.AvatarUrl
        };
    }
}
