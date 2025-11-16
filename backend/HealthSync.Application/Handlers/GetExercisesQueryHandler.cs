using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, List<ExerciseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExercisesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExerciseDto>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(request.MuscleGroup))
        {
            query = query.Where(e => e.MuscleGroup == request.MuscleGroup);
        }

        if (!string.IsNullOrEmpty(request.Difficulty))
        {
            query = query.Where(e => e.Difficulty == request.Difficulty);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(e => e.Name.Contains(request.Search) || (e.Description != null && e.Description.Contains(request.Search)));
        }

        var exercises = await query
            .Select(e => new ExerciseDto
            {
                ExerciseId = e.ExerciseId,
                Name = e.Name,
                MuscleGroup = e.MuscleGroup,
                Difficulty = e.Difficulty,
                Equipment = e.Equipment,
                Description = e.Description
            })
            .ToListAsync(cancellationToken);

        return exercises;
    }
}