using HealthSync.Application.DTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HealthSync.Application.Commands;

public class GoogleLoginWebCommand : IRequest<AuthResponse>
{
    [Required]
    public string Code { get; set; } = string.Empty;
    [Required]
    public string State { get; set; } = string.Empty;
}