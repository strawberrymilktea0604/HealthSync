using HealthSync.Application.DTOs;
using HealthSync.Application.Extensions;
using HealthSync.Application.Queries;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginQueryHandler(
        IApplicationDbContext context, 
        IAuthService authService,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Sai email hoặc mật khẩu!");
        }

        // Kiểm tra nếu user đăng ký qua Google (chưa set password)
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Tài khoản này đăng ký qua Google. Vui lòng đăng nhập bằng Google hoặc đặt mật khẩu trước.");
        }

        // Kiểm tra password
        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Sai email hoặc mật khẩu!");
        }

        // Get user roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToList();

        // Generate JWT token with permissions
        var tokenDto = await _jwtTokenService.GenerateTokenAsync(
            user.UserId, 
            user.Email, 
            roles, 
            permissions);

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        _context.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? user.Email,
            Role = user.GetRoleName(),
            Token = tokenDto.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            IsProfileComplete = user.Profile?.IsComplete() ?? false,
            Roles = roles,
            Permissions = permissions,
            AvatarUrl = user.Profile?.AvatarUrl
        };
    }
}