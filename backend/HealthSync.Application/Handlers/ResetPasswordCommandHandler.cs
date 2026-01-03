using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using HealthSync.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthSync.Application.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context, 
        IJwtTokenService jwtTokenService,
        IAuthService authService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _authService = authService;
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

        var userIdClaim = principal.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException($"Invalid token: cannot parse user ID from claim 'sub' (Value: {userIdClaim})");
        }

        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Tài khoản của bạn đã bị khóa.");
        }

        // Hash password using AuthService to ensure consistency with Login logic
        user.PasswordHash = _authService.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);
    }
}