using MediatR;

namespace HealthSync.Application.Commands;

public class ResetPasswordCommand : IRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}