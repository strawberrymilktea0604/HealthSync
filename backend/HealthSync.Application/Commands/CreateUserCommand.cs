using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class CreateUserCommand : IRequest<AdminUserDto>
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
