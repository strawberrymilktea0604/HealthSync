using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class CreateGoalCommand : IRequest<GoalResponse>
{
    public int UserId { get; set; } // From JWT
    public string Type { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}