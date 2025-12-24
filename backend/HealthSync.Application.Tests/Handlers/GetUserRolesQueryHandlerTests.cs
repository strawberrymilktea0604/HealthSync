using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetUserRolesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetUserRolesQueryHandler _handler;

    public GetUserRolesQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetUserRolesQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnRoles_WhenUserHasRoles()
    {
        // Arrange
        var role1 = new Role { Id = 1, RoleName = "Customer" };
        var role2 = new Role { Id = 2, RoleName = "Admin" };
        
        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1, Role = role1 },
            new() { UserId = 1, RoleId = 2, Role = role2 }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserRolesQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Customer", result);
        Assert.Contains("Admin", result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoRoles()
    {
        // Arrange
        var userRoles = new List<UserRole>();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserRolesQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyRolesForSpecifiedUser()
    {
        // Arrange
        var role1 = new Role { Id = 1, RoleName = "Customer" };
        var role2 = new Role { Id = 2, RoleName = "Admin" };
        
        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1, Role = role1 },
            new() { UserId = 2, RoleId = 2, Role = role2 }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserRolesQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Customer", result[0]);
        Assert.DoesNotContain("Admin", result);
    }

    [Fact]
    public async Task Handle_ShouldReturnRoleNames_NotRoleIds()
    {
        // Arrange
        var role = new Role { Id = 5, RoleName = "Moderator" };
        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 5, Role = role }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserRolesQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Moderator", result[0]);
        Assert.IsType<string>(result[0]);
    }
}
