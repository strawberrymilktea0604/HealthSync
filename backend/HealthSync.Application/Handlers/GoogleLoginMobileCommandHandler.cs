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

        // Find user by email (works for both Google OAuth users and regular email users)
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == googleUser.Email, cancellationToken);

        if (user == null)
        {
            // Create new user for first-time Google login
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
        else
        {
            // User exists (either from regular registration or previous Google login)
            // Allow login regardless of whether they have a password or not
            // This enables: 
            // 1. Regular email users with Gmail to login via Google
            // 2. Previous Google users to login again
        }

        // Generate JWT token
        var token = _authService.GenerateJwtToken(user);
        
        // Check if user needs to set password (first-time Google login)
        var requiresPassword = string.IsNullOrEmpty(user.PasswordHash);
        
        var response = new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            RequiresPassword = requiresPassword
        };

        return response;
    }
}