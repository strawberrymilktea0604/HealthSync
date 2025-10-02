using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public LoginQueryHandler(IApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Sai email hoặc mật khẩu!");
        }

        // Kiểm tra password
        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Sai email hoặc mật khẩu!");
        }

        // Tạo JWT token
        var token = _authService.GenerateJwtToken(user);

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Match với JWT settings
        };
    }
}