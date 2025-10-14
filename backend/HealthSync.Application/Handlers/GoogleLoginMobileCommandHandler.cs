using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GoogleLoginMobileCommandHandler : IRequestHandler<GoogleLoginMobileCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleLoginMobileCommandHandler(
        IApplicationDbContext context,
        IAuthService authService,
        IGoogleAuthService googleAuthService)
    {
        _context = context;
        _authService = authService;
        _googleAuthService = googleAuthService;
    }

    public async Task<AuthResponse> Handle(GoogleLoginMobileCommand request, CancellationToken cancellationToken)
    {
        // Verify Google ID token
        var googleUser = await _googleAuthService.VerifyIdTokenAsync(request.IdToken);
        if (googleUser == null)
        {
            throw new UnauthorizedAccessException("Invalid Google ID token");
        }

        // Find or create user
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == googleUser.Email, cancellationToken);

        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                Email = googleUser.Email,
                PasswordHash = "", // No password for OAuth users
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Create profile
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = googleUser.Name,
                Dob = DateTime.UtcNow.AddYears(-25), // Default age
                Gender = "Unknown",
                HeightCm = 170, // Default
                WeightKg = 70, // Default
                ActivityLevel = "Moderate"
            };

            _context.Add(profile);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Generate JWT token
        var token = _authService.GenerateJwtToken(user);
        var response = new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        return response;
    }
}