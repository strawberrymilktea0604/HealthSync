using HealthSync.Application.DTOs;
using HealthSync.Application.Extensions;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, AdminUserDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
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

        return new AdminUserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? "",
            Role = user.GetRoleName(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.Profile?.AvatarUrl
        };
    }
}
