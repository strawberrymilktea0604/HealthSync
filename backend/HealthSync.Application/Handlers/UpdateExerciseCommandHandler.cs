using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == request.ExerciseId, cancellationToken);

        if (exercise == null)
        {
            return false;
        }

        exercise.Name = request.Name;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.Difficulty = request.Difficulty;
        exercise.Equipment = request.Equipment;
        exercise.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
