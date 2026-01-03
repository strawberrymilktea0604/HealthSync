using HealthSync.Application.Commands;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class ResendResetOtpCommandHandler : IRequestHandler<ResendResetOtpCommand>
{
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _context;

    public ResendResetOtpCommandHandler(
        IOtpService otpService, 
        IEmailService emailService,
        IApplicationDbContext context)
    {
        _otpService = otpService;
        _emailService = emailService;
        _context = context;
    }

    public async Task Handle(ResendResetOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _context.ApplicationUsers, 
            u => u.Email == request.Email, 
            cancellationToken);

        if (user == null || !user.IsActive)
        {
            return;
        }

        var otp = _otpService.GenerateOtp(request.Email);
        await _emailService.SendVerificationCodeAsync(request.Email, otp);
    }
}