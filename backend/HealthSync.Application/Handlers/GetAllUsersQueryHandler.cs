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

        // Sorting
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "userid":
                    query = request.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.UserId) 
                        : query.OrderBy(u => u.UserId);
                    break;
                case "email":
                    query = request.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.Email) 
                        : query.OrderBy(u => u.Email);
                    break;
                case "fullname":
                    query = request.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.Profile != null ? u.Profile.FullName : "") 
                        : query.OrderBy(u => u.Profile != null ? u.Profile.FullName : "");
                    break;
                case "role":
                     query = request.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.UserRoles.FirstOrDefault().Role.RoleName) 
                        : query.OrderBy(u => u.UserRoles.FirstOrDefault().Role.RoleName);
                    break;
                case "isactive":
                    query = request.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.IsActive) 
                        : query.OrderBy(u => u.IsActive);
                    break;
                case "createdat":
                    query = request.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.CreatedAt) 
                        : query.OrderBy(u => u.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        var users = await query
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
                // FIX: Lấy AvatarUrl từ UserProfiles nếu có, nếu không thì fallback về ApplicationUsers
                AvatarUrl = (u.Profile != null && u.Profile.AvatarUrl != null) ? u.Profile.AvatarUrl : u.AvatarUrl
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
