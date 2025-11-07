using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateGoalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GoalResponse> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = new Goal
        {
            UserId = request.UserId,
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "active",
            Notes = request.Notes
        };

        _context.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        return new GoalResponse
        {
            GoalId = goal.GoalId,
            Type = goal.Type,
            TargetValue = goal.TargetValue,
            StartDate = goal.StartDate,
            EndDate = goal.EndDate,
            Status = goal.Status,
            Notes = goal.Notes,
            ProgressRecords = new List<ProgressRecordResponse>() // Empty for new goal
        };
    }
}