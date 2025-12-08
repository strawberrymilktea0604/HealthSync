using HealthSync.Application.DTOs;
using System.Security.Claims;

namespace HealthSync.Application.Services;

/// <summary>
/// Interface for JWT token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate a JWT token for a user with their roles and permissions
    /// </summary>
    Task<TokenDto> GenerateTokenAsync(int userId, string email, List<string> roles, List<string> permissions);
    
    /// <summary>
    /// Generate a reset password token
    /// </summary>
    Task<string> GenerateResetTokenAsync(int userId, string email);
    
    /// <summary>
    /// Validate a JWT token
    /// </summary>
    bool ValidateToken(string token);
    
    /// <summary>
    /// Get ClaimsPrincipal from token
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}
