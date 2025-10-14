using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
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

    public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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

        // Kiểm tra email đã tồn tại chưa (tương tự logic Java)
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

        // Tạo profile cho user
        var profile = new UserProfile
        {
            UserId = user.UserId,
            FullName = request.FullName,
            Dob = request.DateOfBirth,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            ActivityLevel = "Moderate"
        };

        _context.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return user.UserId;
    }
}