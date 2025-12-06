using HealthSync.Application.DTOs;

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
    /// Validate a JWT token
    /// </summary>
    bool ValidateToken(string token);
}
