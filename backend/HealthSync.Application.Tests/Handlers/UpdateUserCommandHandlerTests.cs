using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateUserCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProfileAndRole_WhenUserExists()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = 1,
            FullName = "Updated Name",
            Role = "Admin"
        };

        var existingProfile = new UserProfile { FullName = "Old Name" };
        var existingRole = new Role { Id = 2, RoleName = "Customer" };
        var targetRole = new Role { Id = 1, RoleName = "Admin" };
        
        var user = new ApplicationUser
        {
            UserId = 1,
            UserName = "test",
            Email = "test@example.com",
            Profile = existingProfile,
            UserRoles = new List<UserRole> 
            { 
                new UserRole { RoleId = 2, Role = existingRole } 
            }
        };

        // Mock Users
        var usersList = new List<ApplicationUser> { user };
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        // Mock Roles
        var rolesList = new List<Role> { existingRole, targetRole };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(rolesMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.FullName);
        // Note: verifying role update relies on how the handler constructs the return DTO.
        // It fetches again: await _context.ApplicationUsers...
        // Since we are using the same in-memory object 'user', modifying it in the handler reflects here.
        
        // Check Profile Update
        Assert.Equal("Updated Name", user.Profile!.FullName);

        // Verify Role Switch
        _contextMock.Verify(c => c.Remove(It.IsAny<UserRole>()), Times.Once); // Removing old role
        _contextMock.Verify(c => c.Add(It.IsAny<UserRole>()), Times.Once); // Adding new role
        
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateProfile_WhenProfileIsNull()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = 1,
            FullName = "New Profile Name",
            Role = "Customer" // Same role, so no role change
        };

        var existingRole = new Role { Id = 1, RoleName = "Customer" };
        
        var user = new ApplicationUser
        {
            UserId = 1,
            Profile = null,
            UserRoles = new List<UserRole> 
            { 
                new UserRole { RoleId = 1, Role = existingRole } 
            }
        };

        var usersList = new List<ApplicationUser> { user };
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        var rolesList = new List<Role> { existingRole };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(rolesMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _contextMock.Verify(c => c.Add(It.IsAny<UserProfile>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserCommand { UserId = 999, Role = "Admin" };
        var usersList = new List<ApplicationUser>();
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("User with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenNewRoleDoesNotExist()
    {
         // Arrange
        var command = new UpdateUserCommand
        {
            UserId = 1,
            FullName = "Name",
            Role = "NonExistentRole"
        };

        var user = new ApplicationUser
        {
            UserId = 1,
            UserRoles = new List<UserRole> 
            { 
                new UserRole { Role = new Role { RoleName = "Customer" } } 
            }
        };

        var usersList = new List<ApplicationUser> { user };
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        var rolesList = new List<Role> { new Role { RoleName = "Customer" } };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(rolesMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
            
        Assert.Contains("Role 'NonExistentRole' not found", exception.Message);
    }
}
