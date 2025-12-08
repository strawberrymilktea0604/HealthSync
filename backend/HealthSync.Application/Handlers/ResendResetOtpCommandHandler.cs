using HealthSync.Application.Commands;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class ResendResetOtpCommandHandler : IRequestHandler<ResendResetOtpCommand>
{
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResendResetOtpCommandHandler(IOtpService otpService, IEmailService emailService)
    {
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task Handle(ResendResetOtpCommand request, CancellationToken cancellationToken)
    {
        var otp = _otpService.GenerateOtp(request.Email);
        await _emailService.SendVerificationCodeAsync(request.Email, otp);
    }
}