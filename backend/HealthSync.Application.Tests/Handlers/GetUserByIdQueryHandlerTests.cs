using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetUserByIdQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            IsActive = true,
            CreatedAt = DateTime.Now,
            Profile = new UserProfile
            {
                FullName = "Test User",
                AvatarUrl = "https://avatar.url"
            },
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = 1, RoleId = 1, Role = role }
            }
        };

        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetUserByIdQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("user@test.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("Customer", result.Role);
        Assert.True(result.IsActive);
        Assert.Equal("https://avatar.url", result.AvatarUrl);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetUserByIdQuery { UserId = 999 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyFullName_WhenProfileIsNull()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            IsActive = true,
            CreatedAt = DateTime.Now,
            Profile = null,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = 1, RoleId = 1, Role = role }
            }
        };

        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetUserByIdQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.FullName);
        Assert.Null(result.AvatarUrl);
    }

    [Fact]
    public async Task Handle_ShouldIncludeProfile_WhenQueryingUser()
    {
        // Arrange
        var role = new Role { Id = 2, RoleName = "Admin" };
        var user = new ApplicationUser
        {
            UserId = 10,
            Email = "admin@test.com",
            IsActive = true,
            CreatedAt = DateTime.Now.AddDays(-30),
            Profile = new UserProfile
            {
                FullName = "Admin User",
                Dob = new DateTime(1990, 1, 1),
                Gender = "Male",
                HeightCm = 180m,
                AvatarUrl = "https://admin.avatar"
            },
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = 10, RoleId = 2, Role = role }
            }
        };

        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetUserByIdQuery { UserId = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal("Admin User", result.FullName);
        Assert.Equal("Admin", result.Role);
        Assert.Equal("https://admin.avatar", result.AvatarUrl);
    }
}
