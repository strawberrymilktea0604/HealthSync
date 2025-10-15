using MediatR;
using HealthSync.Application.DTOs;

namespace HealthSync.Application.Commands;

public class RegisterUserCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}