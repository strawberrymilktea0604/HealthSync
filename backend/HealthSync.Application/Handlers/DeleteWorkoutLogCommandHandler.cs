using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class DeleteWorkoutLogCommandHandler : IRequestHandler<DeleteWorkoutLogCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkoutLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .FirstOrDefaultAsync(w => w.WorkoutLogId == request.WorkoutLogId, cancellationToken);

        if (log == null)
        {
            return false;
        }

        // Security Check: IDOR
        if (log.UserId != request.UserId)
        {
            // User is trying to delete someone else's log
            throw new UnauthorizedAccessException("Bạn không có quyền xóa nhật ký này.");
        }

        _context.Remove(log);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
