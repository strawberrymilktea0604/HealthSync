using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class CreateWorkoutLogCommandHandler : IRequestHandler<CreateWorkoutLogCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkoutLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        var workoutLog = new WorkoutLog
        {
            UserId = request.UserId,
            WorkoutDate = request.WorkoutLog.WorkoutDate,
            DurationMin = request.WorkoutLog.DurationMin,
            Notes = request.WorkoutLog.Notes,
            ExerciseSessions = request.WorkoutLog.ExerciseSessions.Select(es => new ExerciseSession
            {
                ExerciseId = es.ExerciseId,
                Sets = es.Sets,
                Reps = es.Reps,
                WeightKg = es.WeightKg,
                RestSec = es.RestSec,
                Rpe = es.Rpe
            }).ToList()
        };

        _context.Add(workoutLog);
        await _context.SaveChangesAsync(cancellationToken);

        return workoutLog.WorkoutLogId;
    }
}