using MediatR;

namespace HealthSync.Application.Commands;

public class VerifyResetOtpCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}