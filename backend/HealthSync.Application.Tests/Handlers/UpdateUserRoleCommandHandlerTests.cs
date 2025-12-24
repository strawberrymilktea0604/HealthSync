using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateUserRoleCommandHandler _handler;

    public UpdateUserRoleCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateUserRoleCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserRole_WhenUserAndRoleExist()
    {
        // Arrange
        var oldRole = new Role { Id = 1, RoleName = "Customer" };
        var newRole = new Role { Id = 2, RoleName = "Admin" };
        var userRole = new UserRole { UserId = 1, RoleId = 1, Role = oldRole };
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Profile = new UserProfile { FullName = "Test User", AvatarUrl = "avatar.jpg" },
            UserRoles = new List<UserRole> { userRole }
        };

        var users = new List<ApplicationUser> { user };
        var roles = new List<Role> { oldRole, newRole };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Remove(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.Add(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateUserRoleCommand { UserId = 1, Role = "Admin" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Admin", result.Role);
        Assert.Equal("user@test.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        _contextMock.Verify(c => c.Remove(userRole), Times.Once);
        _contextMock.Verify(c => c.Add(It.Is<UserRole>(ur => ur.UserId == 1 && ur.RoleId == 2)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var roles = new List<Role> { new() { Id = 2, RoleName = "Admin" } };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());

        var command = new UpdateUserRoleCommand { UserId = 999, Role = "Admin" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("User with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoleNotFound()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            UserRoles = new List<UserRole>()
        };
        var users = new List<ApplicationUser> { user };
        var roles = new List<Role>();

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());

        var command = new UpdateUserRoleCommand { UserId = 1, Role = "NonExistentRole" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Role 'NonExistentRole' not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldHandleUserWithNoExistingRole()
    {
        // Arrange
        var newRole = new Role { Id = 2, RoleName = "Admin" };
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Profile = null,
            UserRoles = new List<UserRole>()
        };

        var users = new List<ApplicationUser> { user };
        var roles = new List<Role> { newRole };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Add(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateUserRoleCommand { UserId = 1, Role = "Admin" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Admin", result.Role);
        Assert.Equal("", result.FullName); // Null profile returns empty string
        Assert.Null(result.AvatarUrl);
        _contextMock.Verify(c => c.Remove(It.IsAny<UserRole>()), Times.Never);
        _contextMock.Verify(c => c.Add(It.Is<UserRole>(ur => ur.UserId == 1 && ur.RoleId == 2)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectAdminUserDto()
    {
        // Arrange
        var newRole = new Role { Id = 2, RoleName = "Admin" };
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var user = new ApplicationUser
        {
            UserId = 5,
            Email = "admin@test.com",
            IsActive = true,
            CreatedAt = createdAt,
            Profile = new UserProfile { FullName = "Admin User", AvatarUrl = "admin.jpg" },
            UserRoles = new List<UserRole>()
        };

        var users = new List<ApplicationUser> { user };
        var roles = new List<Role> { newRole };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Roles).Returns(roles.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Add(It.IsAny<UserRole>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateUserRoleCommand { UserId = 5, Role = "Admin" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.UserId);
        Assert.Equal("admin@test.com", result.Email);
        Assert.Equal("Admin User", result.FullName);
        Assert.Equal("Admin", result.Role);
        Assert.True(result.IsActive);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.Equal("admin.jpg", result.AvatarUrl);
    }
}
