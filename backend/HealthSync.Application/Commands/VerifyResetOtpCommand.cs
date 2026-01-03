using MediatR;

namespace HealthSync.Application.Commands;

using HealthSync.Application.DTOs;

public class VerifyResetOtpCommand : IRequest<VerifyResetOtpResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}