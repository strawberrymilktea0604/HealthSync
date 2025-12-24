using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using System.Linq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class LoginQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly LoginQueryHandler _handler;

    public LoginQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _authServiceMock = new Mock<IAuthService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new LoginQueryHandler(_contextMock.Object, _authServiceMock.Object, _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>(); // Empty list
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var query = new LoginQuery { Email = "nonexistent@example.com", Password = "password123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));
        Assert.Contains("Sai email hoặc mật khẩu", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserInactive()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                IsActive = false // Inactive
            }
        };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var query = new LoginQuery { Email = "user@example.com", Password = "password123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));
        Assert.Contains("Sai email hoặc mật khẩu", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserRegisteredViaGoogle_NoPassword()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "googleuser@example.com",
                PasswordHash = string.Empty, // No password set for OAuth users
                IsActive = true,
                Profile = new UserProfile { FullName = "Google User" },
                UserRoles = new List<UserRole>()
            }
        };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var query = new LoginQuery { Email = "googleuser@example.com", Password = "password123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));
        Assert.Contains("đăng ký qua Google", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordIncorrect()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                Profile = new UserProfile { FullName = "Test User" },
                UserRoles = new List<UserRole>()
            }
        };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        _authServiceMock.Setup(a => a.VerifyPassword("wrongpassword", "hashedpassword")).Returns(false);

        var query = new LoginQuery { Email = "user@example.com", Password = "wrongpassword" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));
        Assert.Contains("Sai email hoặc mật khẩu", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenLoginSuccessful()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "User", RolePermissions = new List<RolePermission>() };
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true,
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
            Profile = new UserProfile 
            { 
                FullName = "Test User",
                Gender = "Male",
                HeightCm = 175.5m,
                WeightKg = 70.0m,
                Dob = DateTime.UtcNow.AddYears(-15)
            },
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = role }
            }
        };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        _authServiceMock.Setup(a => a.VerifyPassword("password123", "hashedpassword")).Returns(true);

        var tokenDto = new TokenDto { AccessToken = "jwt-token-123" };
        _jwtTokenServiceMock.Setup(j => j.GenerateTokenAsync(1, "user@example.com", It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(tokenDto);

        var query = new LoginQuery { Email = "user@example.com", Password = "password123" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("jwt-token-123", result.Token);
        Assert.True(result.IsProfileComplete);
        Assert.Contains("User", result.Roles);

        // Verify last login updated
        _contextMock.Verify(c => c.Update(user), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleCaseInsensitiveEmail()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "User@Example.Com", // Upper case
                PasswordHash = "hashedpassword",
                IsActive = true,
                Profile = new UserProfile { FullName = "Test User" },
                UserRoles = new List<UserRole>()
            }
        };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        _authServiceMock.Setup(a => a.VerifyPassword("password123", "hashedpassword")).Returns(true);

        var tokenDto = new TokenDto { AccessToken = "jwt-token-123" };
        _jwtTokenServiceMock.Setup(j => j.GenerateTokenAsync(1, "User@Example.Com", It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(tokenDto);

        var query = new LoginQuery { Email = "user@example.com", Password = "password123" }; // Lower case

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDbContextFails()
    {
        // Arrange
        _contextMock.Setup(c => c.ApplicationUsers).Throws(new Exception("Database error"));

        var query = new LoginQuery { Email = "user@example.com", Password = "password123" };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
    }
}