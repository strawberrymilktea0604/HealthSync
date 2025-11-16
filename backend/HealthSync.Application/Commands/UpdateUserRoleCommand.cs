using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateUserRoleCommand : IRequest<AdminUserDto>
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
