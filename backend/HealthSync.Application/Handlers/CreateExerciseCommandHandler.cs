using HealthSync.Application.Commands;
using HealthSync.Application.Extensions;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        // Note: In real implementation, user should be extracted from JWT claims
        // For demo, we'll assume user is passed or retrieved from context
        // Here we'll skip user check for simplicity, but in production:

        var exercise = new Exercise
        {
            Name = request.Name,
            MuscleGroup = request.MuscleGroup,
            Difficulty = request.Difficulty,
            Equipment = request.Equipment,
            Description = request.Description
        };

        _context.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return exercise.ExerciseId;
    }
}