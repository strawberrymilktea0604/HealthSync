using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetUserPermissionsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetUserPermissionsQueryHandler _handler;

    public GetUserPermissionsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetUserPermissionsQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPermissions_WhenUserHasRolesWithPermissions()
    {
        // Arrange
        var permission1 = new Permission { Id = 1, PermissionCode = "VIEW_USERS" };
        var permission2 = new Permission { Id = 2, PermissionCode = "EDIT_USERS" };
        
        var role = new Role 
        { 
            Id = 1, 
            RoleName = "Admin",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 1, PermissionId = 1, Permission = permission1 },
                new() { RoleId = 1, PermissionId = 2, Permission = permission2 }
            }
        };

        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1, Role = role }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserPermissionsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("VIEW_USERS", result);
        Assert.Contains("EDIT_USERS", result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoPermissions()
    {
        // Arrange
        var userRoles = new List<UserRole>();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserPermissionsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldCombinePermissionsFromMultipleRoles()
    {
        // Arrange
        var permission1 = new Permission { Id = 1, PermissionCode = "VIEW_USERS" };
        var permission2 = new Permission { Id = 2, PermissionCode = "EDIT_POSTS" };
        
        var role1 = new Role 
        { 
            Id = 1, 
            RoleName = "Admin",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 1, PermissionId = 1, Permission = permission1 }
            }
        };

        var role2 = new Role 
        { 
            Id = 2, 
            RoleName = "Moderator",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 2, PermissionId = 2, Permission = permission2 }
            }
        };

        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1, Role = role1 },
            new() { UserId = 1, RoleId = 2, Role = role2 }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserPermissionsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("VIEW_USERS", result);
        Assert.Contains("EDIT_POSTS", result);
    }

    [Fact]
    public async Task Handle_ShouldReturnDistinctPermissions()
    {
        // Arrange
        var permission = new Permission { Id = 1, PermissionCode = "VIEW_USERS" };
        
        var role1 = new Role 
        { 
            Id = 1, 
            RoleName = "Admin",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 1, PermissionId = 1, Permission = permission }
            }
        };

        var role2 = new Role 
        { 
            Id = 2, 
            RoleName = "Moderator",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 2, PermissionId = 1, Permission = permission }
            }
        };

        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1, Role = role1 },
            new() { UserId = 1, RoleId = 2, Role = role2 }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserPermissionsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result); // Should not duplicate
        Assert.Equal("VIEW_USERS", result[0]);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyPermissionsForSpecifiedUser()
    {
        // Arrange
        var permission1 = new Permission { Id = 1, PermissionCode = "VIEW_USERS" };
        var permission2 = new Permission { Id = 2, PermissionCode = "DELETE_USERS" };
        
        var role1 = new Role 
        { 
            Id = 1, 
            RoleName = "Customer",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 1, PermissionId = 1, Permission = permission1 }
            }
        };

        var role2 = new Role 
        { 
            Id = 2, 
            RoleName = "Admin",
            RolePermissions = new List<RolePermission>
            {
                new() { RoleId = 2, PermissionId = 2, Permission = permission2 }
            }
        };

        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1, Role = role1 },
            new() { UserId = 2, RoleId = 2, Role = role2 }
        };

        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.AsQueryable().BuildMock());

        var query = new GetUserPermissionsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("VIEW_USERS", result[0]);
        Assert.DoesNotContain("DELETE_USERS", result);
    }
}
