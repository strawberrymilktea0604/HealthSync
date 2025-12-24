using System.Security.Claims;
using HealthSync.Application.Commands;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HealthSync.Presentation.Tests.Controllers;

public class UserManagementControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UserManagementController _controller;

    public UserManagementControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UserManagementController(_mediatorMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task AssignRole_WithValidData_ReturnsOk()
    {
        // Arrange
        var userId = 10;
        var roleId = 2;

        _mediatorMock
            .Setup(m => m.Send(It.Is<AssignRoleToUserCommand>(c => c.UserId == userId && c.RoleId == roleId), default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignRole(userId, roleId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task AssignRole_WhenAlreadyAssigned_ReturnsBadRequest()
    {
        // Arrange
        var userId = 10;
        var roleId = 2;

        _mediatorMock
            .Setup(m => m.Send(It.Is<AssignRoleToUserCommand>(c => c.UserId == userId && c.RoleId == roleId), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignRole(userId, roleId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task AssignRole_WithInvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var userId = -1;
        var roleId = 2;

        _mediatorMock
            .Setup(m => m.Send(It.Is<AssignRoleToUserCommand>(c => c.UserId == userId && c.RoleId == roleId), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignRole(userId, roleId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RemoveRole_WithValidData_ReturnsOk()
    {
        // Arrange
        var userId = 10;
        var roleId = 2;

        _mediatorMock
            .Setup(m => m.Send(It.Is<RemoveRoleFromUserCommand>(c => c.UserId == userId && c.RoleId == roleId), default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveRole(userId, roleId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task RemoveRole_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 10;
        var roleId = 999;

        _mediatorMock
            .Setup(m => m.Send(It.Is<RemoveRoleFromUserCommand>(c => c.UserId == userId && c.RoleId == roleId), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveRole(userId, roleId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task RemoveRole_WithInvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var userId = -1;
        var roleId = 2;

        _mediatorMock
            .Setup(m => m.Send(It.Is<RemoveRoleFromUserCommand>(c => c.UserId == userId && c.RoleId == roleId), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveRole(userId, roleId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserRoles_WithValidUserId_ReturnsOkWithRoles()
    {
        // Arrange
        var userId = 10;
        var expectedRoles = new List<string> { "Customer", "Admin" };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetUserRolesQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _controller.GetUserRoles(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Verify roles are in response
        var responseType = okResult.Value!.GetType();
        var rolesProperty = responseType.GetProperty("roles");
        Assert.NotNull(rolesProperty);
        var roles = (List<string>)rolesProperty.GetValue(okResult.Value)!;
        Assert.Equal(2, roles.Count);
        Assert.Contains("Customer", roles);
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public async Task GetUserRoles_WithNoRoles_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userId = 10;
        var expectedRoles = new List<string>();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetUserRolesQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _controller.GetUserRoles(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var responseType = okResult.Value!.GetType();
        var rolesProperty = responseType.GetProperty("roles");
        Assert.NotNull(rolesProperty);
        var roles = (List<string>)rolesProperty.GetValue(okResult.Value)!;
        Assert.Empty(roles);
    }

    [Fact]
    public async Task GetUserPermissions_WithValidUserId_ReturnsOkWithPermissions()
    {
        // Arrange
        var userId = 10;
        var expectedPermissions = new List<string> { "USER_READ", "USER_UPDATE", "EXERCISE_READ" };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetUserPermissionsQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(expectedPermissions);

        // Act
        var result = await _controller.GetUserPermissions(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var responseType = okResult.Value!.GetType();
        var permissionsProperty = responseType.GetProperty("permissions");
        Assert.NotNull(permissionsProperty);
        var permissions = (List<string>)permissionsProperty.GetValue(okResult.Value)!;
        Assert.Equal(3, permissions.Count);
        Assert.Contains("USER_READ", permissions);
    }

    [Fact]
    public async Task GetUserPermissions_WithNoPermissions_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userId = 10;
        var expectedPermissions = new List<string>();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetUserPermissionsQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(expectedPermissions);

        // Act
        var result = await _controller.GetUserPermissions(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var responseType = okResult.Value!.GetType();
        var permissionsProperty = responseType.GetProperty("permissions");
        Assert.NotNull(permissionsProperty);
        var permissions = (List<string>)permissionsProperty.GetValue(okResult.Value)!;
        Assert.Empty(permissions);
    }

    [Fact]
    public async Task GetUserPermissions_WithInvalidUserId_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userId = -1;
        var expectedPermissions = new List<string>();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetUserPermissionsQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(expectedPermissions);

        // Act
        var result = await _controller.GetUserPermissions(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}
