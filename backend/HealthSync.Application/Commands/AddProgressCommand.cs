using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class AddProgressCommand : IRequest<AddProgressResponse>
{
    public int GoalId { get; set; }
    public int UserId { get; set; } // From JWT, to verify ownership
    public DateTime RecordDate { get; set; }
    public decimal Value { get; set; }
    public string? Notes { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
}