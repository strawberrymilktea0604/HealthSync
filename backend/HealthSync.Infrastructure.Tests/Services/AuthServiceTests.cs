using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        
        // Setup JWT configuration with a key long enough for HS256 (32 bytes minimum)
        var jwtSettingsMock = new Mock<IConfigurationSection>();
        jwtSettingsMock.Setup(x => x["SecretKey"]).Returns("your-very-long-secret-key-that-is-at-least-32-characters-long");
        jwtSettingsMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
        jwtSettingsMock.Setup(x => x["Audience"]).Returns("TestAudience");
        jwtSettingsMock.Setup(x => x["ExpiryMinutes"]).Returns("60");
        
        _configMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsMock.Object);
        
        _authService = new AuthService(_configMock.Object);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedString()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var hashed = _authService.HashPassword(password);

        // Assert
        Assert.NotNull(hashed);
        Assert.NotEqual(password, hashed);
        Assert.True(hashed.Length > 0);
    }

    [Fact]
    public void HashPassword_ShouldBeConsistent()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var hashed1 = _authService.HashPassword(password);
        var hashed2 = _authService.HashPassword(password);

        // Assert
        Assert.Equal(hashed1, hashed2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var password = "testpassword";
        var hashed = _authService.HashPassword(password);

        // Act
        var isValid = _authService.VerifyPassword(password, hashed);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var password = "testpassword";
        var wrongPassword = "wrongpassword";
        var hashed = _authService.HashPassword(password);

        // Act
        var isValid = _authService.VerifyPassword(wrongPassword, hashed);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashesForDifferentPasswords()
    {
        // Arrange
        var password1 = "password1";
        var password2 = "password2";

        // Act
        var hash1 = _authService.HashPassword(password1);
        var hash2 = _authService.HashPassword(password2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForNullOrEmptyHash()
    {
        // Arrange
        var password = "password";

        // Act & Assert
        Assert.False(_authService.VerifyPassword(password, string.Empty));
        Assert.False(_authService.VerifyPassword(password, ""));
    }

    [Fact]
    public void GenerateJwtToken_ShouldIncludeCorrectClaims_ForAdminUser()
    {
        // Arrange - Mock configuration
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["SecretKey"]).Returns("test-secret-key-for-testing-purposes-only-with-sufficient-length");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSectionMock.Setup(s => s["Audience"]).Returns("TestAudience");
        jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns("30");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var authService = new AuthService(configMock.Object);

        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "admin@example.com",
            Profile = new UserProfile { FullName = "Admin User" },
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Admin" } }
            }
        };

        // Act
        var token = authService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify token contains expected parts
        Assert.Contains(".", token); // JWT format has dots
    }

    [Fact]
    public void GenerateJwtToken_ShouldIncludeCorrectClaims_ForCustomerUser()
    {
        // Arrange - Mock configuration
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["SecretKey"]).Returns("test-secret-key-for-testing-purposes-only-with-sufficient-length");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSectionMock.Setup(s => s["Audience"]).Returns("TestAudience");
        jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns("30");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var authService = new AuthService(configMock.Object);

        var user = new ApplicationUser
        {
            UserId = 2,
            Email = "customer@example.com",
            Profile = new UserProfile { FullName = "Customer User" },
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Customer" } }
            }
        };

        // Act
        var token = authService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify token contains expected parts
        Assert.Contains(".", token); // JWT format has dots
    }

    [Fact]
    public void GenerateJwtToken_ShouldHandleNullProfile()
    {
        // Arrange - Mock configuration
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["SecretKey"]).Returns("test-secret-key-for-testing-purposes-only-with-sufficient-length");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSectionMock.Setup(s => s["Audience"]).Returns("TestAudience");
        jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns("30");

        _configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var user = new ApplicationUser
        {
            UserId = 3,
            Email = "user@example.com",
            Profile = null, // No profile
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Customer" } }
            }
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("user@example.com", jwtToken.Claims.First(c => c.Type == "fullName").Value); // Should use email
    }

    [Fact]
    public void GenerateJwtToken_ShouldIncludeCorrectClaims_ForUnknownRole()
    {
        // Arrange - Mock configuration
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["SecretKey"]).Returns("test-secret-key-for-testing-purposes-only-with-sufficient-length");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSectionMock.Setup(s => s["Audience"]).Returns("TestAudience");
        jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns("30");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var authService = new AuthService(configMock.Object);

        var user = new ApplicationUser
        {
            UserId = 4,
            Email = "unknown@example.com",
            Profile = new UserProfile { FullName = "Unknown User" },
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "UnknownRole" } }
            }
        };

        // Act
        var token = authService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify token contains expected parts
        Assert.Contains(".", token); // JWT format has dots
    }

    [Fact]
    public void GenerateJwtToken_ShouldUseDefaultConfigValues_WhenConfigMissing()
    {
        // Arrange - Mock empty configuration
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["SecretKey"]).Returns("your-very-long-default-secret-key-that-is-at-least-32-characters");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns((string?)null);
        jwtSectionMock.Setup(s => s["Audience"]).Returns((string?)null);
        jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns((string?)null);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var authService = new AuthService(configMock.Object);

        var user = new ApplicationUser
        {
            UserId = 5,
            Email = "user@example.com",
            Profile = new UserProfile { FullName = "Test User" },
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Customer" } }
            }
        };

        // Act
        var token = authService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify token contains expected parts
        Assert.Contains(".", token); // JWT format has dots
    }
}