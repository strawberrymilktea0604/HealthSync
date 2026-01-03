using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using HealthSync.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IOtpService otpService,
        IEmailService emailService)
    {
        _context = context;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user == null)
        {
             throw new KeyNotFoundException("Email không tồn tại trong hệ thống.");
        }

        if (!user.IsActive)
        {
             throw new KeyNotFoundException("Tài khoản của bạn đã bị khóa.");
        }

        var otp = _otpService.GenerateOtp(user.Email);
        await _emailService.SendResetPasswordOtpAsync(user.Email, otp);
    }
}