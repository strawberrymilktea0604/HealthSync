using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateUserPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly UpdateUserPasswordCommandHandler _handler;

    public UpdateUserPasswordCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IApplicationUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _handler = new UpdateUserPasswordCommandHandler(_userRepositoryMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePassword_WhenUserExists()
    {
        // Arrange
        var command = new UpdateUserPasswordCommand
        {
            UserId = 1,
            NewPassword = "NewSecretPassword123"
        };

        var user = new ApplicationUser
        {
            UserId = 1,
            PasswordHash = "old_hash"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _authServiceMock.Setup(a => a.HashPassword("NewSecretPassword123"))
            .Returns("new_hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("new_hashed_password", user.PasswordHash);
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        var command = new UpdateUserPasswordCommand
        {
            UserId = 999,
            NewPassword = "Password"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}
