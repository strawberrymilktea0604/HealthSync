using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
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
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new Exception($"User with ID {request.UserId} not found");
        }

        if (request.Role != "Admin" && request.Role != "Customer")
        {
            throw new Exception("Invalid role. Must be 'Admin' or 'Customer'");
        }

        user.Role = request.Role;
        _context.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? "",
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.Profile?.AvatarUrl
        };
    }
}
