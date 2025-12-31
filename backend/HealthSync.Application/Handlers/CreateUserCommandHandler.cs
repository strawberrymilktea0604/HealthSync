using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, AdminUserDto>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Email '{request.Email}' is already registered");
        }

        // Get role
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role, cancellationToken);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role '{request.Role}' not found");
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email.Split('@')[0],
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Create user profile
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

        // Assign role
        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = role.Id
        };
        _context.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            FullName = request.FullName,
            Role = request.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.AvatarUrl
        };
    }
}
