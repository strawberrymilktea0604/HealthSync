using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class AddProgressCommandHandler : IRequestHandler<AddProgressCommand, AddProgressResponse>
{
    private readonly IApplicationDbContext _context;

    public AddProgressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AddProgressResponse> Handle(AddProgressCommand request, CancellationToken cancellationToken)
    {
        // Verify the goal belongs to the user
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.GoalId == request.GoalId && g.UserId == request.UserId, cancellationToken);

        if (goal == null)
        {
            throw new KeyNotFoundException("Goal not found or does not belong to the user.");
        }

        var progressRecord = new ProgressRecord
        {
            GoalId = request.GoalId,
            RecordDate = request.RecordDate,
            Value = request.Value,
            Notes = request.Notes,
            WeightKg = request.WeightKg ?? 0,
            WaistCm = request.WaistCm ?? 0
        };

        _context.Add(progressRecord);
        await _context.SaveChangesAsync(cancellationToken);

        return new AddProgressResponse
        {
            ProgressRecord = new ProgressRecordResponse
            {
                ProgressRecordId = progressRecord.ProgressRecordId,
                RecordDate = progressRecord.RecordDate,
                Value = progressRecord.Value,
                Notes = progressRecord.Notes,
                WeightKg = progressRecord.WeightKg,
                WaistCm = progressRecord.WaistCm
            }
        };
    }
}