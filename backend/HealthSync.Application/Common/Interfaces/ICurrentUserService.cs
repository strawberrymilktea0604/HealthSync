namespace HealthSync.Application.Common.Interfaces;

/// <summary>
/// Service to get current authenticated user information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID (from JWT claims)
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Gets the current user's email (from JWT claims)
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Checks if the user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
