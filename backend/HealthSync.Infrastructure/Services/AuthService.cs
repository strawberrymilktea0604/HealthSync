using HealthSync.Application.Extensions;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HealthSync.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }

    public string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration?.GetSection("Jwt");
        // Default key must be at least 256 bits (32 characters) for HS256
        var secretKey = jwtSettings?["SecretKey"] ?? "your-default-secret-key-min-32-chars-for-hs256-algorithm";
        var issuer = jwtSettings?["Issuer"] ?? "HealthSync";
        var audience = jwtSettings?["Audience"] ?? "HealthSyncUsers";
        var expiryMinutesStr = jwtSettings?["ExpiryMinutes"] ?? "60";
        var expiryMinutes = int.TryParse(expiryMinutesStr, out var minutes) ? minutes : 60;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.Profile?.FullName ?? user.Email),
            new Claim("role", user.GetRoleName()), // Use short "role" instead of full URL
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add permissions based on role
        var permissions = GetPermissionsForRole(user.GetRoleName());
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static List<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            RoleNames.ADMIN => new List<string>
            {
                // User Management
                PermissionCodes.USER_READ,
                PermissionCodes.USER_BAN,
                PermissionCodes.USER_UPDATE_ROLE,
                PermissionCodes.USER_DELETE,

                // Exercise Management
                PermissionCodes.EXERCISE_READ,
                PermissionCodes.EXERCISE_CREATE,
                PermissionCodes.EXERCISE_UPDATE,
                PermissionCodes.EXERCISE_DELETE,

                // Food Management
                PermissionCodes.FOOD_READ,
                PermissionCodes.FOOD_CREATE,
                PermissionCodes.FOOD_UPDATE,
                PermissionCodes.FOOD_DELETE,

                // Workout Logs
                PermissionCodes.WORKOUT_LOG_READ,
                PermissionCodes.WORKOUT_LOG_CREATE,
                PermissionCodes.WORKOUT_LOG_UPDATE, // Assuming Admin can update all logs
                PermissionCodes.WORKOUT_LOG_DELETE,

                // Nutrition Logs
                PermissionCodes.NUTRITION_LOG_READ,
                PermissionCodes.NUTRITION_LOG_CREATE,
                PermissionCodes.NUTRITION_LOG_UPDATE,
                PermissionCodes.NUTRITION_LOG_DELETE,

                // Goals
                PermissionCodes.GOAL_READ,
                PermissionCodes.GOAL_CREATE,
                PermissionCodes.GOAL_UPDATE,
                PermissionCodes.GOAL_DELETE,

                // Dashboard & Reports
                PermissionCodes.DASHBOARD_ADMIN,
                PermissionCodes.DASHBOARD_VIEW, // Admin can view user dashboard too? or just admin dashboard logic
                // PermissionCodes.REPORTS_VIEW, // Not defined in PermissionCodes.cs yet? Check file.
            },
            RoleNames.CUSTOMER => new List<string>
            {
                // View Library
                PermissionCodes.EXERCISE_READ,
                PermissionCodes.FOOD_READ,

                // Own Workout Logs (Permissions are generic, Policy handles "Own")
                PermissionCodes.WORKOUT_LOG_READ,
                PermissionCodes.WORKOUT_LOG_CREATE,
                PermissionCodes.WORKOUT_LOG_UPDATE,
                PermissionCodes.WORKOUT_LOG_DELETE,

                // Own Nutrition Logs
                PermissionCodes.NUTRITION_LOG_READ,
                PermissionCodes.NUTRITION_LOG_CREATE,
                PermissionCodes.NUTRITION_LOG_UPDATE,
                PermissionCodes.NUTRITION_LOG_DELETE,

                // Own Goals
                PermissionCodes.GOAL_READ,
                PermissionCodes.GOAL_CREATE,
                PermissionCodes.GOAL_UPDATE,
                PermissionCodes.GOAL_DELETE,

                // Dashboard
                PermissionCodes.DASHBOARD_VIEW,
            },
            _ => new List<string>()
        };
    }
}