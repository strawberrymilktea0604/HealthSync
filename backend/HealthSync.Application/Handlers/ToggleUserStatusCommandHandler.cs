using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.CurrentUserId)
        {
            throw new InvalidOperationException("Cannot change status of your own account");
        }

        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
