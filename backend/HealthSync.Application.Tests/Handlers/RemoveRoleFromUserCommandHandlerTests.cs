using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class RemoveRoleFromUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly RemoveRoleFromUserCommandHandler _handler;

    public RemoveRoleFromUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new RemoveRoleFromUserCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveRoleFromUser_WhenAssignmentExists()
    {
        // Arrange
        var userRole = new UserRole { UserId = 1, RoleId = 2, AssignedAt = DateTime.UtcNow };
        var userRoles = new List<UserRole> { userRole };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Remove(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RemoveRoleFromUserCommand { UserId = 1, RoleId = 2 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Remove(userRole), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var userRoles = new List<UserRole>();

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var command = new RemoveRoleFromUserCommand { UserId = 1, RoleId = 2 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<UserRole>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldOnlyRemoveSpecificUserRole()
    {
        // Arrange
        var userRole1 = new UserRole { UserId = 1, RoleId = 2 };
        var userRole2 = new UserRole { UserId = 1, RoleId = 3 };
        var userRole3 = new UserRole { UserId = 2, RoleId = 2 };
        var userRoles = new List<UserRole> { userRole1, userRole2, userRole3 };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Remove(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RemoveRoleFromUserCommand { UserId = 1, RoleId = 2 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.Remove(userRole1), Times.Once);
        _contextMock.Verify(c => c.Remove(userRole2), Times.Never);
        _contextMock.Verify(c => c.Remove(userRole3), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserIdMatches_ButRoleIdDoesNot()
    {
        // Arrange
        var userRole = new UserRole { UserId = 1, RoleId = 2 };
        var userRoles = new List<UserRole> { userRole };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var command = new RemoveRoleFromUserCommand { UserId = 1, RoleId = 999 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenRoleIdMatches_ButUserIdDoesNot()
    {
        // Arrange
        var userRole = new UserRole { UserId = 1, RoleId = 2 };
        var userRoles = new List<UserRole> { userRole };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var command = new RemoveRoleFromUserCommand { UserId = 999, RoleId = 2 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<UserRole>()), Times.Never);
    }
}
