using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Extensions;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GoogleLoginMobileCommandHandler : IRequestHandler<GoogleLoginMobileCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtTokenService _jwtTokenService;

    public GoogleLoginMobileCommandHandler(
        IApplicationDbContext context,
        IGoogleAuthService googleAuthService,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _googleAuthService = googleAuthService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(GoogleLoginMobileCommand request, CancellationToken cancellationToken)
    {
        // Verify Google ID token
        var googleUser = await _googleAuthService.VerifyIdTokenAsync(request.IdToken);
        if (googleUser == null)
        {
            throw new UnauthorizedAccessException("Invalid Google ID token");
        }

        // Check if email belongs to an Admin account - BLOCK Google login for Admins
        var existingAdminUser = await _context.ApplicationUsers
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == googleUser.Email && u.UserRoles.Any(ur => ur.Role.RoleName == "Admin"), cancellationToken);
        
        if (existingAdminUser != null)
        {
            throw new UnauthorizedAccessException("Tài khoản Admin không được phép đăng nhập qua Google. Vui lòng sử dụng email và mật khẩu.");
        }

        // Find user by email (works for both Google OAuth users and regular email users)
        var user = await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == googleUser.Email, cancellationToken);

        if (user == null)
        {
            // Create new user for first-time Google login
            user = new ApplicationUser
            {
                Email = googleUser.Email,
                PasswordHash = "", // No password for OAuth users
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Get Customer role
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer", cancellationToken);
            if (customerRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = customerRole.Id
                };
                _context.Add(userRole);
                await _context.SaveChangesAsync(cancellationToken);
            }

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

        // Extract roles and permissions
        var roles = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToList();

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToList();

        // Generate JWT token with permissions
        var tokenDto = await _jwtTokenService.GenerateTokenAsync(
            user.UserId,
            user.Email,
            roles,
            permissions);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        
        // Check if user needs to set password (first-time Google login)
        var requiresPassword = string.IsNullOrEmpty(user.PasswordHash);
        
        var response = new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.Profile?.FullName ?? user.Email,
            Role = user.GetRoleName(),
            Token = tokenDto.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenDto.ExpiresIn),
            Roles = tokenDto.Roles,
            Permissions = tokenDto.Permissions,
            RequiresPassword = requiresPassword,
            IsProfileComplete = user.Profile?.IsComplete() ?? false
        };

        return response;
    }
}