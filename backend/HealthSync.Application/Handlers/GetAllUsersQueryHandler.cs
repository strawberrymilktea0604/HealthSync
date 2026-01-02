using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Application.Extensions;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        query = ApplyFilters(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.SortBy, request.SortOrder);

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

    private static IQueryable<ApplicationUser> ApplyFilters(IQueryable<ApplicationUser> query, GetAllUsersQuery request)
    {
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

        return query;
    }

    private static IQueryable<ApplicationUser> ApplySorting(IQueryable<ApplicationUser> query, string? sortBy, string? sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query.OrderByDescending(u => u.CreatedAt);
        }

        bool isDesc = sortOrder?.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "userid" => ApplyOrder(query, u => u.UserId, isDesc),
            "email" => ApplyOrder(query, u => u.Email, isDesc),
            "fullname" => ApplyOrder(query, u => u.Profile != null ? u.Profile.FullName : "", isDesc),
            "role" => ApplyOrder(query, u => u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault(), isDesc),
            "isactive" => ApplyOrder(query, u => u.IsActive, isDesc),
            "createdat" => ApplyOrder(query, u => u.CreatedAt, isDesc),
            _ => query.OrderByDescending(u => u.CreatedAt),
        };
    }

    private static IQueryable<ApplicationUser> ApplyOrder<TKey>(IQueryable<ApplicationUser> query, System.Linq.Expressions.Expression<Func<ApplicationUser, TKey>> keySelector, bool isDesc)
    {
        return isDesc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
