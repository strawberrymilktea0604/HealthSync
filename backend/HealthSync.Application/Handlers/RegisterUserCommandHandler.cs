using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Extensions;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserCommandHandler(
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

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Verify the email code first
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

        // Kiểm tra email đã tồn tại chưa
        var existingUser = await _context.ApplicationUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            // Nếu email đã tồn tại và là Admin, báo lỗi đặc biệt
            if (existingUser.GetRoleName() == "Admin")
            {
                throw new InvalidOperationException("Email này đã được đăng ký với tài khoản Admin. Vui lòng liên hệ quản trị viên.");
            }
            throw new InvalidOperationException("Email đã tồn tại!");
        }

        // Tạo user mới
        var user = new ApplicationUser
        {
            Email = request.Email,
            PasswordHash = _authService.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign Customer role
        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer", cancellationToken);
        if (customerRole != null)
        {
            var userRole = new UserRole { UserId = user.UserId, RoleId = customerRole.Id };
            _context.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Reload user with roles and permissions
        user = await _context.ApplicationUsers
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == user.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("Failed to reload user after creation");
        }

        // Tạo profile rỗng cho user (sẽ điền sau trong dashboard)
        var profile = new UserProfile
        {
            UserId = user.UserId,
            FullName = "", // Will be filled later
            Dob = DateTime.UtcNow.AddYears(-25), // Default
            Gender = "Unknown",
            HeightCm = 0,
            WeightKg = 0,
            ActivityLevel = "Moderate"
        };

        _context.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions
        var roles = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToList();

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

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Email, // Use email as temp name
            Role = user.GetRoleName(),
            Token = tokenDto.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenDto.ExpiresIn),
            Roles = tokenDto.Roles,
            Permissions = tokenDto.Permissions,
            IsProfileComplete = profile.IsComplete()
        };
    }
}