using HealthSync.Application.Commands;
using HealthSync.Application.Services;
using MediatR;

namespace HealthSync.Application.Handlers;

public class VerifyResetOtpCommandHandler : IRequestHandler<VerifyResetOtpCommand, bool>
{
    private readonly IOtpService _otpService;

    public VerifyResetOtpCommandHandler(IOtpService otpService)
    {
        _otpService = otpService;
    }

    public async Task<bool> Handle(VerifyResetOtpCommand request, CancellationToken cancellationToken)
    {
        var result = _otpService.ValidateOtp(request.Email, request.Otp);
        await Task.CompletedTask;
        return result;
    }
}