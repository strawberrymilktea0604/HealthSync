using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GoogleLoginWebCommandHandler : IRequestHandler<GoogleLoginWebCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleLoginWebCommandHandler(
        IApplicationDbContext context,
        IAuthService authService,
        IGoogleAuthService googleAuthService)
    {
        _context = context;
        _authService = authService;
        _googleAuthService = googleAuthService;
    }

    public async Task<AuthResponse> Handle(GoogleLoginWebCommand request, CancellationToken cancellationToken)
    {
        // Process Google OAuth callback
        var googleUser = await _googleAuthService.ProcessCallbackAsync(request.Code);
        if (googleUser == null)
        {
            throw new UnauthorizedAccessException("Invalid Google authorization code");
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

            // Create profile with Google info
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = googleUser.Name ?? googleUser.Email,
                AvatarUrl = googleUser.Picture ?? "",
                Dob = DateTime.UtcNow.AddYears(-25), // Default age
                Gender = "Unknown",
                HeightCm = 0, // Will be filled later
                WeightKg = 0, // Will be filled later
                ActivityLevel = "Moderate"
            };

            _context.Add(profile);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload user with profile
            user = await _context.ApplicationUsers
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId, cancellationToken);
        }
        else
        {
            // User exists (either from regular registration or previous Google login)
            // Update profile with latest Google info if available
            if (user.Profile != null && !string.IsNullOrEmpty(googleUser.Picture))
            {
                user.Profile.AvatarUrl = googleUser.Picture;
                if (string.IsNullOrEmpty(user.Profile.FullName) && !string.IsNullOrEmpty(googleUser.Name))
                {
                    user.Profile.FullName = googleUser.Name;
                }
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Ensure user and profile are loaded
        if (user == null)
        {
            throw new InvalidOperationException("Failed to load user data");
        }

        // Generate JWT token
        var token = _authService.GenerateJwtToken(user);
        
        // Check if user needs to set password (first-time Google login)
        var requiresPassword = string.IsNullOrEmpty(user.PasswordHash);
        
        var response = new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? googleUser.Name ?? user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            RequiresPassword = requiresPassword
        };

        return response;
    }
}