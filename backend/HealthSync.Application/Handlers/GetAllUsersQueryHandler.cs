using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Application.Extensions;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, AdminUsersResponse>
{
    private readonly IApplicationDbContext _context;

    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUsersResponse> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                (u.Profile != null && u.Profile.FullName.ToLower().Contains(searchLower))
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != "All Roles")
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == request.Role));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new AdminUserListDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.Profile != null ? u.Profile.FullName : "",
                Role = u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault() ?? "Customer",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                // FIX: Lấy AvatarUrl từ UserProfiles thay vì ApplicationUsers
                AvatarUrl = u.Profile != null ? u.Profile.AvatarUrl : u.AvatarUrl
            })
            .ToListAsync(cancellationToken);

        return new AdminUsersResponse
        {
            Users = users,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
