using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class UpdateExerciseImageCommandHandler : IRequestHandler<UpdateExerciseImageCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateExerciseImageCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == request.ExerciseId, cancellationToken);

        if (exercise == null)
        {
            throw new InvalidOperationException("Không tìm thấy bài tập");
        }

        exercise.ImageUrl = request.ImageUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
