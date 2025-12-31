using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateUserPasswordCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
