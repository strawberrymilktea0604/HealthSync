using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetExerciseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExerciseDto?> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .Where(e => e.ExerciseId == request.ExerciseId)
            .Select(e => new ExerciseDto
            {
                ExerciseId = e.ExerciseId,
                Name = e.Name,
                MuscleGroup = e.MuscleGroup,
                Difficulty = e.Difficulty,
                Equipment = e.Equipment,
                Description = e.Description
            })
            .FirstOrDefaultAsync(cancellationToken);

        return exercise;
    }
}
