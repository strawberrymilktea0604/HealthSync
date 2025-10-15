using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HealthSync.Application.Commands;

public class SetPasswordCommand : IRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}