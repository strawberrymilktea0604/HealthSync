using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class AssignRoleToUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly AssignRoleToUserCommandHandler _handler;

    public AssignRoleToUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new AssignRoleToUserCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAssignRole_WhenUserAndRoleExist()
    {
        // Arrange
        var users = new List<ApplicationUser> { new() { UserId = 1, Email = "user@test.com" } };
        var roles = new List<Role> { new() { Id = 2, RoleName = "Admin" } };
        var userRoles = new List<UserRole>();

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Add(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AssignRoleToUserCommand { UserId = 1, RoleId = 2 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Add(It.Is<UserRole>(ur => ur.UserId == 1 && ur.RoleId == 2)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var roles = new List<Role> { new() { Id = 2, RoleName = "Admin" } };
        var userRoles = new List<UserRole>();

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var command = new AssignRoleToUserCommand { UserId = 999, RoleId = 2 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("User with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoleNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser> { new() { UserId = 1, Email = "user@test.com" } };
        var roles = new List<Role>();
        var userRoles = new List<UserRole>();

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var command = new AssignRoleToUserCommand { UserId = 1, RoleId = 999 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Role with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenRoleAlreadyAssigned()
    {
        // Arrange
        var users = new List<ApplicationUser> { new() { UserId = 1, Email = "user@test.com" } };
        var roles = new List<Role> { new() { Id = 2, RoleName = "Admin" } };
        var userRoles = new List<UserRole> 
        { 
            new() { UserId = 1, RoleId = 2, AssignedAt = DateTime.UtcNow } 
        };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var command = new AssignRoleToUserCommand { UserId = 1, RoleId = 2 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Add(It.IsAny<UserRole>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSetAssignedAtDate()
    {
        // Arrange
        var users = new List<ApplicationUser> { new() { UserId = 1, Email = "user@test.com" } };
        var roles = new List<Role> { new() { Id = 2, RoleName = "Admin" } };
        var userRoles = new List<UserRole>();

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Add(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AssignRoleToUserCommand { UserId = 1, RoleId = 2 };
        var beforeTime = DateTime.UtcNow;

        // Act
        await _handler.Handle(command, CancellationToken.None);
        var afterTime = DateTime.UtcNow;

        // Assert
        _contextMock.Verify(c => c.Add(It.Is<UserRole>(ur => 
            ur.AssignedAt >= beforeTime && ur.AssignedAt <= afterTime)), Times.Once);
    }
}
