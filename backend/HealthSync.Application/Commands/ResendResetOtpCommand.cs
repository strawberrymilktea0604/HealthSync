using MediatR;

namespace HealthSync.Application.Commands;

public class ResendResetOtpCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
}