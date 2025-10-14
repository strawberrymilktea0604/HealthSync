using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class GoogleLoginWebCommand : IRequest<AuthResponse>
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}