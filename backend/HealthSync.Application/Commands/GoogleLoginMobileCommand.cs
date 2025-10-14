using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class GoogleLoginMobileCommand : IRequest<AuthResponse>
{
    public string IdToken { get; set; } = string.Empty;
}