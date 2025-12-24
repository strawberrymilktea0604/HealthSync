using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new ResetPasswordCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldResetPassword_WhenTokenIsValid()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = "old-hash"
        };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("type", "reset"),
            new Claim(ClaimTypes.Email, "user@test.com")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _jwtTokenServiceMock.Setup(j => j.GetPrincipalFromToken("valid-token"))
            .Returns(principal);

        var command = new ResetPasswordCommand { Token = "valid-token", NewPassword = "NewPassword123!" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual("old-hash", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword123!", user.PasswordHash));
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTokenIsInvalid()
    {
        // Arrange
        _jwtTokenServiceMock.Setup(j => j.GetPrincipalFromToken("invalid-token"))
            .Returns((ClaimsPrincipal?)null);

        var command = new ResetPasswordCommand { Token = "invalid-token", NewPassword = "NewPassword123!" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Invalid or expired reset token", exception.Message);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTokenTypeIsNotReset()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("type", "access"), // Wrong type
            new Claim(ClaimTypes.Email, "user@test.com")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _jwtTokenServiceMock.Setup(j => j.GetPrincipalFromToken("wrong-type-token"))
            .Returns(principal);

        var command = new ResetPasswordCommand { Token = "wrong-type-token", NewPassword = "NewPassword123!" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Invalid token type", exception.Message);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim("type", "reset"),
            new Claim(ClaimTypes.Email, "notfound@test.com")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _jwtTokenServiceMock.Setup(j => j.GetPrincipalFromToken("valid-token"))
            .Returns(principal);

        var command = new ResetPasswordCommand { Token = "valid-token", NewPassword = "NewPassword123!" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("User not found", exception.Message);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIdClaimIsInvalid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number"),
            new Claim("type", "reset")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _jwtTokenServiceMock.Setup(j => j.GetPrincipalFromToken("invalid-user-id-token"))
            .Returns(principal);

        var command = new ResetPasswordCommand { Token = "invalid-user-id-token", NewPassword = "NewPassword123!" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Invalid token", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_WithBCrypt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = "old-hash"
        };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("type", "reset")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _jwtTokenServiceMock.Setup(j => j.GetPrincipalFromToken("valid-token"))
            .Returns(principal);

        var command = new ResetPasswordCommand { Token = "valid-token", NewPassword = "SecurePassword123!" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.StartsWith("$2", user.PasswordHash); // BCrypt hashes start with $2
        Assert.True(BCrypt.Net.BCrypt.Verify("SecurePassword123!", user.PasswordHash));
    }
}
