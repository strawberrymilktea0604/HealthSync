using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public SetPasswordCommandHandler(
        IApplicationDbContext context,
        IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Tài khoản của bạn đã bị khóa.");
        }

        // Hash the password
        user.PasswordHash = _authService.HashPassword(request.Password);

        await _context.SaveChangesAsync(cancellationToken);
    }
}