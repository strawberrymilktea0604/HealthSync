using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAllUsersQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUsers_WhenNoFiltersApplied()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "user1@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-2),
                Profile = new UserProfile { FullName = "User One" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 1, RoleId = 1, Role = role } }
            },
            new ApplicationUser
            {
                UserId = 2,
                Email = "user2@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-1),
                Profile = new UserProfile { FullName = "User Two" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 2, RoleId = 1, Role = role } }
            }
        };

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Users.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm_WhenSearchTermProvided()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "john@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "John Doe" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 1, RoleId = 1, Role = role } }
            },
            new ApplicationUser
            {
                UserId = 2,
                Email = "jane@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "Jane Smith" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 2, RoleId = 1, Role = role } }
            }
        };

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { SearchTerm = "john", Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Users);
        Assert.Equal("john@test.com", result.Users[0].Email);
    }

    [Fact]
    public async Task Handle_ShouldFilterByRole_WhenRoleProvided()
    {
        // Arrange
        var customerRole = new Role { Id = 1, RoleName = "Customer" };
        var adminRole = new Role { Id = 2, RoleName = "Admin" };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "customer@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "Customer User" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 1, RoleId = 1, Role = customerRole } }
            },
            new ApplicationUser
            {
                UserId = 2,
                Email = "admin@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "Admin User" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 2, RoleId = 2, Role = adminRole } }
            }
        };

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { Role = "Admin", Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Users);
        Assert.Equal("Admin", result.Users[0].Role);
    }

    [Fact]
    public async Task Handle_ShouldPaginateResults_WhenPageSizeProvided()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var users = new List<ApplicationUser>();
        for (int i = 1; i <= 15; i++)
        {
            users.Add(new ApplicationUser
            {
                UserId = i,
                Email = $"user{i}@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-i),
                Profile = new UserProfile { FullName = $"User {i}" },
                UserRoles = new List<UserRole> { new UserRole { UserId = i, RoleId = 1, Role = role } }
            });
        }

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { Page = 2, PageSize = 5 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(5, result.Users.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "old@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-10),
                Profile = new UserProfile { FullName = "Old User" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 1, RoleId = 1, Role = role } }
            },
            new ApplicationUser
            {
                UserId = 2,
                Email = "new@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "New User" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 2, RoleId = 1, Role = role } }
            }
        };

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@test.com", result.Users[0].Email); // Newest first
        Assert.Equal("old@test.com", result.Users[1].Email);
    }

    [Fact]
    public async Task Handle_ShouldNotFilterByRole_WhenRoleIsAllRoles()
    {
        // Arrange
        var role1 = new Role { Id = 1, RoleName = "Customer" };
        var role2 = new Role { Id = 2, RoleName = "Admin" };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "customer@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "Customer" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 1, RoleId = 1, Role = role1 } }
            },
            new ApplicationUser
            {
                UserId = 2,
                Email = "admin@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = new UserProfile { FullName = "Admin" },
                UserRoles = new List<UserRole> { new UserRole { UserId = 2, RoleId = 2, Role = role2 } }
            }
        };

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { Role = "All Roles", Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount); // Should return all users
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyFullName_WhenProfileIsNull()
    {
        // Arrange
        var role = new Role { Id = 1, RoleName = "Customer" };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserId = 1,
                Email = "noProfile@test.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Profile = null,
                UserRoles = new List<UserRole> { new UserRole { UserId = 1, RoleId = 1, Role = role } }
            }
        };

        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Users);
        Assert.Equal("", result.Users[0].FullName);
    }
}
