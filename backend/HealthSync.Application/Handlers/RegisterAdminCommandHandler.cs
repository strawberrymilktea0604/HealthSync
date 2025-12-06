using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Extensions;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterAdminCommandHandler(
        IApplicationDbContext context,
        IAuthService authService,
        IMediator mediator,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _authService = authService;
        _mediator = mediator;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
    {
        // BƯỚC 1: Kiểm tra xem hệ thống đã có Admin chưa
        var existingAdmin = await _context.ApplicationUsers
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Admin"), cancellationToken);

        if (existingAdmin != null)
        {
            throw new InvalidOperationException("Hệ thống đã có tài khoản Admin. Không thể đăng ký Admin mới qua API này!");
        }

        // BƯỚC 2: Xác thực email code
        var verifyCommand = new VerifyEmailCodeCommand
        {
            Email = request.Email,
            Code = request.VerificationCode
        };

        var isVerified = await _mediator.Send(verifyCommand, cancellationToken);
        if (!isVerified)
        {
            throw new InvalidOperationException("Mã xác thực không hợp lệ!");
        }

        // BƯỚC 3: Kiểm tra email đã tồn tại chưa
        var existingUser = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email đã tồn tại!");
        }

        // BƯỚC 4: Tạo user với Role = Admin
        var admin = new ApplicationUser
        {
            Email = request.Email,
            PasswordHash = _authService.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(admin);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign Admin role
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin", cancellationToken);
        if (adminRole != null)
        {
            var userRole = new UserRole
            {
                UserId = admin.UserId,
                RoleId = adminRole.Id
            };
            _context.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // BƯỚC 5: Tạo profile cho Admin
        var profile = new UserProfile
        {
            UserId = admin.UserId,
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? "System Administrator" : request.FullName,
            Dob = DateTime.UtcNow.AddYears(-30), // Default age 30
            Gender = "Unknown",
            HeightCm = 0,
            WeightKg = 0,
            ActivityLevel = "Moderate"
        };

        _context.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        // BƯỚC 6: Generate JWT token và return
        // Reload admin with roles and permissions
        admin = await _context.ApplicationUsers
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == admin.UserId, cancellationToken);

        if (admin == null)
        {
            throw new InvalidOperationException("Failed to reload admin after creation");
        }

        // Extract roles and permissions
        var roles = admin.UserRoles
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToList();

        var permissions = admin.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToList();

        // Generate JWT token with permissions
        var tokenDto = await _jwtTokenService.GenerateTokenAsync(
            admin.UserId,
            admin.Email,
            roles,
            permissions);

        return new AuthResponse
        {
            UserId = admin.UserId,
            Email = admin.Email,
            FullName = profile.FullName,
            Role = admin.GetRoleName(),
            Token = tokenDto.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenDto.ExpiresIn),
            Roles = tokenDto.Roles,
            Permissions = tokenDto.Permissions,
            IsProfileComplete = profile.IsComplete()
        };
    }
}
