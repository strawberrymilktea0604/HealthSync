using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using HealthSync.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;

namespace HealthSync.Application.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Validate reset token
        var principal = _jwtTokenService.GetPrincipalFromToken(request.Token);
        if (principal == null)
        {
            throw new InvalidOperationException("Invalid or expired reset token");
        }

        var typeClaim = principal.FindFirst("type")?.Value;
        if (typeClaim != "reset")
        {
            throw new InvalidOperationException("Invalid token type");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException("Invalid token");
        }

        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);
    }
}