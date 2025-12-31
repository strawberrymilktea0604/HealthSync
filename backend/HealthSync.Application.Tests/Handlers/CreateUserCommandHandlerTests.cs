using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateUserCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "Password123",
            FullName = "Test User",
            Role = "Admin"
        };

        // Mock ApplicationUsers to return empty (no existing user)
        var usersList = new List<ApplicationUser>();
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        // Mock Roles to return the requested role
        var rolesList = new List<Role> { new Role { Id = 1, RoleName = "Admin" } };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(rolesMock.Object);

        // Setup Add methods to verify calls
        _contextMock.Setup(c => c.Add(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(u => 
            {
                u.UserId = 1; // Simulate DB generating ID
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("Admin", result.Role);

        _contextMock.Verify(c => c.Add(It.IsAny<ApplicationUser>()), Times.Once);
        _contextMock.Verify(c => c.Add(It.IsAny<UserProfile>()), Times.Once); // Should create profile
        _contextMock.Verify(c => c.Add(It.IsAny<UserRole>()), Times.Once);   // Should assign role
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            Password = "Password123",
            FullName = "Test User",
            Role = "Customer"
        };

        // Mock ApplicationUsers to return an existing user
        var usersList = new List<ApplicationUser> 
        { 
            new ApplicationUser { UserId = 1, Email = "existing@example.com" } 
        };
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("already registered", exception.Message);
        
        _contextMock.Verify(c => c.Add(It.IsAny<ApplicationUser>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "new@example.com",
            Password = "Password123",
            FullName = "Test User",
            Role = "SuperAdmin"
        };

        // Mock ApplicationUsers (empty)
        var usersList = new List<ApplicationUser>();
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(usersMock.Object);

        // Mock Roles (empty or just not containing SuperAdmin)
        var rolesList = new List<Role> { new Role { Id = 1, RoleName = "Admin" } };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(rolesMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Role 'SuperAdmin' not found", exception.Message);

        _contextMock.Verify(c => c.Add(It.IsAny<ApplicationUser>()), Times.Never);
    }
}
