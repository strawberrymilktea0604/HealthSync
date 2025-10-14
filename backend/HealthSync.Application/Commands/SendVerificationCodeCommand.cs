using MediatR;

namespace HealthSync.Application.Commands;

public class SendVerificationCodeCommand : IRequest<Unit>
{
    public string Email { get; set; } = string.Empty;
}