using MediatR;

namespace HealthSync.Application.Commands;

public class DeleteUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
}
