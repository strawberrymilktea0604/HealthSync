using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class LoginQuery : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}