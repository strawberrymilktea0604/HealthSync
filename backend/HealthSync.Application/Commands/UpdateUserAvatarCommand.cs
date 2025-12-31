using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateUserAvatarCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
}