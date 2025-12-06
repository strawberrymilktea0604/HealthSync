using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == request.ExerciseId, cancellationToken);

        if (exercise == null)
        {
            return false;
        }

        // Check if exercise is being used in any exercise sessions
        var isUsed = await _context.ExerciseSessions
            .AnyAsync(es => es.ExerciseId == request.ExerciseId, cancellationToken);

        if (isUsed)
        {
            throw new InvalidOperationException("Không thể xóa bài tập đang được sử dụng trong nhật ký luyện tập.");
        }

        _context.Remove(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
