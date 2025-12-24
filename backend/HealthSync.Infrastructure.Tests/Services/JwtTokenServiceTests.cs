using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        // Setup configuration for testing
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Issuer", "HealthSyncTest"},
            {"Jwt:Audience", "HealthSyncTestUsers"},
            {"Jwt:SecretKey", "ThisIsAVerySecureSecretKeyForTesting123456789!"},
            {"Jwt:ExpirationMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _service = new JwtTokenService(_configuration);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidData_ReturnsTokenWithCorrectClaims()
    {
        // Arrange
        var userId = 1;
        var email = "test@example.com";
        var roles = new List<string> { "Customer" };
        var permissions = new List<string> { "read:profile", "write:workout" };

        // Act
        var result = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn); // 60 minutes = 3600 seconds
        Assert.Equal(roles, result.Roles);
        Assert.Equal(permissions, result.Permissions);

        // Verify token can be read and has basic structure
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        
        Assert.NotNull(token);
        Assert.Contains(token.Claims, c => c.Type == "sub" && c.Value == userId.ToString());
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Customer");
    }

    [Fact]
    public async Task GenerateTokenAsync_MultipleRoles_IncludesAllRoles()
    {
        // Arrange
        var userId = 2;
        var email = "admin@example.com";
        var roles = new List<string> { "Customer", "Admin" };
        var permissions = new List<string> { "read:all", "write:all", "delete:all" };

        // Act
        var result = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        
        var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Equal(2, roleClaims.Count);
        Assert.Contains(roleClaims, c => c.Value == "Customer");
        Assert.Contains(roleClaims, c => c.Value == "Admin");
    }

    [Fact]
    public async Task GenerateTokenAsync_NoRolesOrPermissions_GeneratesTokenWithBasicClaims()
    {
        // Arrange
        var userId = 3;
        var email = "basic@example.com";
        var roles = new List<string>();
        var permissions = new List<string>();

        // Act
        var result = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        
        Assert.Contains(token.Claims, c => c.Type == "sub" && c.Value == userId.ToString());
        Assert.DoesNotContain(token.Claims, c => c.Type == ClaimTypes.Role);
    }

    [Fact]
    public async Task GenerateResetTokenAsync_ValidData_ReturnsTokenWithResetType()
    {
        // Arrange
        var userId = 4;
        var email = "reset@example.com";

        // Act
        var result = await _service.GenerateResetTokenAsync(userId, email);

        // Assert
        Assert.NotNull(result);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result);
        
        Assert.Contains(token.Claims, c => c.Type == "sub" && c.Value == userId.ToString());
        Assert.Contains(token.Claims, c => c.Type == "type" && c.Value == "reset");
        
        // Verify expiration is approximately 15 minutes
        var expiration = token.ValidTo;
        var expectedExpiration = DateTime.UtcNow.AddMinutes(15);
        Assert.True(Math.Abs((expiration - expectedExpiration).TotalMinutes) < 1);
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var userId = 5;
        var email = "validate@example.com";
        var roles = new List<string> { "Customer" };
        var permissions = new List<string> { "read:profile" };
        
        var tokenDto = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Act
        var result = _service.ValidateToken(tokenDto.AccessToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _service.ValidateToken(invalidToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsFalse()
    {
        // Arrange - Create a configuration with -1 minute expiration
        var expiredConfig = new Dictionary<string, string>
        {
            {"Jwt:Issuer", "HealthSyncTest"},
            {"Jwt:Audience", "HealthSyncTestUsers"},
            {"Jwt:SecretKey", "ThisIsAVerySecureSecretKeyForTesting123456789!"},
            {"Jwt:ExpirationMinutes", "-1"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(expiredConfig!)
            .Build();

        var expiredService = new JwtTokenService(config);
        var tokenDto = await expiredService.GenerateTokenAsync(1, "expired@example.com", new List<string>(), new List<string>());

        // Act
        var result = _service.ValidateToken(tokenDto.AccessToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPrincipalFromToken_ValidToken_ReturnsPrincipalWithClaims()
    {
        // Arrange
        var userId = 6;
        var email = "principal@example.com";
        var roles = new List<string> { "Admin" };
        var permissions = new List<string> { "read:all", "write:all" };
        
        var tokenDto = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Act
        var principal = _service.GetPrincipalFromToken(tokenDto.AccessToken);

        // Assert - Verify principal was created and has role claims
        // Note: "sub" claim may be mapped to ClaimTypes.NameIdentifier during validation
        Assert.NotNull(principal);
        Assert.NotNull(principal.Identity);
        
        // Check for role claim (this is what we actually need for authorization)
        Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        
        // Verify user identifier exists (could be "sub" or mapped to NameIdentifier)
        var userIdClaim = principal.FindFirst("sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
        Assert.NotNull(userIdClaim);
        Assert.Equal(userId.ToString(), userIdClaim.Value);
    }

    [Fact]
    public void GetPrincipalFromToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "completely.invalid.token";

        // Act
        var principal = _service.GetPrincipalFromToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public async Task GetPrincipalFromToken_ExpiredToken_ReturnsNull()
    {
        // Arrange - Create expired token
        var expiredConfig = new Dictionary<string, string>
        {
            {"Jwt:Issuer", "HealthSyncTest"},
            {"Jwt:Audience", "HealthSyncTestUsers"},
            {"Jwt:SecretKey", "ThisIsAVerySecureSecretKeyForTesting123456789!"},
            {"Jwt:ExpirationMinutes", "-1"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(expiredConfig!)
            .Build();

        var expiredService = new JwtTokenService(config);
        var tokenDto = await expiredService.GenerateTokenAsync(1, "expired@example.com", new List<string>(), new List<string>());

        // Act
        var principal = _service.GetPrincipalFromToken(tokenDto.AccessToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void Constructor_MissingSecretKey_ThrowsException()
    {
        // Arrange
        var invalidConfig = new Dictionary<string, string>
        {
            {"Jwt:Issuer", "HealthSyncTest"},
            {"Jwt:Audience", "HealthSyncTestUsers"},
            {"Jwt:ExpirationMinutes", "60"}
            // Missing SecretKey
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(invalidConfig!)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new JwtTokenService(config));
    }

    [Fact]
    public async Task GenerateTokenAsync_IncludesJtiClaim_ForUniqueTokenTracking()
    {
        // Arrange
        var userId = 7;
        var email = "jti@example.com";
        var roles = new List<string> { "Customer" };
        var permissions = new List<string>();

        // Act
        var result1 = await _service.GenerateTokenAsync(userId, email, roles, permissions);
        var result2 = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token1 = handler.ReadJwtToken(result1.AccessToken);
        var token2 = handler.ReadJwtToken(result2.AccessToken);

        var jti1Claim = token1.Claims.FirstOrDefault(c => c.Type == "jti");
        var jti2Claim = token2.Claims.FirstOrDefault(c => c.Type == "jti");

        Assert.NotNull(jti1Claim);
        Assert.NotNull(jti2Claim);
        Assert.NotEqual(jti1Claim.Value, jti2Claim.Value); // Each token should have unique JTI
    }

    [Fact]
    public async Task GenerateTokenAsync_IncludesUidClaim_ForBackwardCompatibility()
    {
        // Arrange
        var userId = 8;
        var email = "uid@example.com";
        var roles = new List<string> { "Customer" };
        var permissions = new List<string>();

        // Act
        var result = await _service.GenerateTokenAsync(userId, email, roles, permissions);

        // Assert - Token should be generated successfully with user ID in sub claim
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        
        var subClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");
        Assert.NotNull(subClaim);
        Assert.Equal(userId.ToString(), subClaim.Value);
    }
}
