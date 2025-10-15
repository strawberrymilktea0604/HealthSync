using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
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

    public RegisterUserCommandHandler(IApplicationDbContext context, IAuthService authService, IMediator mediator)
    {
        _context = context;
        _authService = authService;
        _mediator = mediator;
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
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email đã tồn tại!");
        }

        // Tạo user mới
        var user = new ApplicationUser
        {
            Email = request.Email,
            PasswordHash = _authService.HashPassword(request.Password),
            Role = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

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

        // Generate JWT token and return
        var token = _authService.GenerateJwtToken(user);

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Email, // Use email as temp name
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}