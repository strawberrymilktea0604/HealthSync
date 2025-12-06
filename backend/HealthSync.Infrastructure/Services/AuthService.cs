using HealthSync.Application.Extensions;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
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
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "your-secret-key-here";
        var issuer = jwtSettings["Issuer"] ?? "HealthSync";
        var audience = jwtSettings["Audience"] ?? "HealthSyncUsers";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.Profile?.FullName ?? user.Email),
            new Claim(ClaimTypes.Role, user.GetRoleName()),
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

    private List<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "Admin" => new List<string>
            {
                // All User Management
                "Permissions.Users.View",
                "Permissions.Users.Create",
                "Permissions.Users.Edit",
                "Permissions.Users.Delete",
                "Permissions.Users.ManageRoles",

                // All Exercise Management
                "Permissions.Exercises.View",
                "Permissions.Exercises.Create",
                "Permissions.Exercises.Edit",
                "Permissions.Exercises.Delete",

                // All Food Management
                "Permissions.FoodItems.View",
                "Permissions.FoodItems.Create",
                "Permissions.FoodItems.Edit",
                "Permissions.FoodItems.Delete",

                // View All Data
                "Permissions.WorkoutLogs.ViewAll",
                "Permissions.NutritionLogs.ViewAll",
                "Permissions.Goals.ViewAll",
                "Permissions.Profile.ViewAll",

                // Dashboard & Reports
                "Permissions.Dashboard.ViewAdmin",
                "Permissions.Reports.View",
                "Permissions.Reports.Export",
            },
            "Customer" => new List<string>
            {
                // View Library
                "Permissions.Exercises.View",
                "Permissions.FoodItems.View",

                // Own Workout Logs
                "Permissions.WorkoutLogs.ViewOwn",
                "Permissions.WorkoutLogs.CreateOwn",
                "Permissions.WorkoutLogs.EditOwn",
                "Permissions.WorkoutLogs.DeleteOwn",

                // Own Nutrition Logs
                "Permissions.NutritionLogs.ViewOwn",
                "Permissions.NutritionLogs.CreateOwn",
                "Permissions.NutritionLogs.EditOwn",
                "Permissions.NutritionLogs.DeleteOwn",

                // Own Goals
                "Permissions.Goals.ViewOwn",
                "Permissions.Goals.CreateOwn",
                "Permissions.Goals.EditOwn",
                "Permissions.Goals.DeleteOwn",

                // Own Profile
                "Permissions.Profile.ViewOwn",
                "Permissions.Profile.EditOwn",
            },
            _ => new List<string>()
        };
    }
}