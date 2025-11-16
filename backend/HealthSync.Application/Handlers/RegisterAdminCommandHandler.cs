using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
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

    public RegisterAdminCommandHandler(IApplicationDbContext context, IAuthService authService, IMediator mediator)
    {
        _context = context;
        _authService = authService;
        _mediator = mediator;
    }

    public async Task<AuthResponse> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
    {
        // BƯỚC 1: Kiểm tra xem hệ thống đã có Admin chưa
        var existingAdmin = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Role == "Admin", cancellationToken);

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
            Role = "Admin", // Đặt Role là Admin
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(admin);
        await _context.SaveChangesAsync(cancellationToken);

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
        var token = _authService.GenerateJwtToken(admin);

        return new AuthResponse
        {
            UserId = admin.UserId,
            Email = admin.Email,
            FullName = profile.FullName,
            Role = admin.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}
