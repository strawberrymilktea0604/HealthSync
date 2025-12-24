using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteUserCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser { UserId = 1, Email = "user@test.com" };
        var users = new List<ApplicationUser> { user };
        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Remove(It.IsAny<ApplicationUser>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteUserCommand { UserId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Remove(user), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());

        var command = new DeleteUserCommand { UserId = 999 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("User with ID 999 not found", exception.Message);
        _contextMock.Verify(c => c.Remove(It.IsAny<ApplicationUser>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRemoveCorrectUser()
    {
        // Arrange
        var user1 = new ApplicationUser { UserId = 1, Email = "user1@test.com" };
        var user2 = new ApplicationUser { UserId = 2, Email = "user2@test.com" };
        var users = new List<ApplicationUser> { user1, user2 };
        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Remove(It.IsAny<ApplicationUser>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteUserCommand { UserId = 2 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.Remove(user2), Times.Once);
        _contextMock.Verify(c => c.Remove(user1), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAlwaysReturnTrue_WhenSuccessful()
    {
        // Arrange
        var user = new ApplicationUser { UserId = 1, Email = "user@test.com" };
        var users = new List<ApplicationUser> { user };
        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Remove(It.IsAny<ApplicationUser>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteUserCommand { UserId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.IsType<bool>(result);
    }
}
