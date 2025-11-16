using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetWorkoutLogsQueryHandler : IRequestHandler<GetWorkoutLogsQuery, List<WorkoutLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkoutLogDto>> Handle(GetWorkoutLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkoutLogs
            .Where(w => w.UserId == request.UserId)
            .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(w => w.WorkoutDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(w => w.WorkoutDate <= request.EndDate.Value);
        }

        var workoutLogs = await query
            .Include(w => w.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
            .Select(w => new WorkoutLogDto
            {
                WorkoutLogId = w.WorkoutLogId,
                UserId = w.UserId,
                WorkoutDate = w.WorkoutDate,
                DurationMin = w.DurationMin,
                Notes = w.Notes,
                ExerciseSessions = w.ExerciseSessions.Select(es => new ExerciseSessionDto
                {
                    ExerciseSessionId = es.ExerciseSessionId,
                    ExerciseId = es.ExerciseId,
                    ExerciseName = es.Exercise.Name,
                    Sets = es.Sets,
                    Reps = es.Reps,
                    WeightKg = es.WeightKg,
                    RestSec = es.RestSec,
                    Rpe = es.Rpe
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return workoutLogs;
    }
}