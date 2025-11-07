using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetGoalsQueryHandler : IRequestHandler<GetGoalsQuery, GetGoalsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetGoalsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetGoalsResponse> Handle(GetGoalsQuery request, CancellationToken cancellationToken)
    {
        var goals = await _context.Goals
            .Where(g => g.UserId == request.UserId)
            .Include(g => g.ProgressRecords)
            .ToListAsync(cancellationToken);

        var goalResponses = goals.Select(g => new GoalResponse
        {
            GoalId = g.GoalId,
            Type = g.Type,
            TargetValue = g.TargetValue,
            StartDate = g.StartDate,
            EndDate = g.EndDate,
            Status = g.Status,
            Notes = g.Notes,
            ProgressRecords = g.ProgressRecords.Select(pr => new ProgressRecordResponse
            {
                ProgressRecordId = pr.ProgressRecordId,
                RecordDate = pr.RecordDate,
                Value = pr.Value,
                Notes = pr.Notes,
                WeightKg = pr.WeightKg,
                WaistCm = pr.WaistCm
            }).ToList()
        }).ToList();

        return new GetGoalsResponse
        {
            Goals = goalResponses
        };
    }
}