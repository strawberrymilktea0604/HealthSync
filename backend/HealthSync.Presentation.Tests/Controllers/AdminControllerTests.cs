using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<AdminController>> _loggerMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AdminController>>();
        _controller = new AdminController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllUsers_WithValidParameters_ReturnsOkWithUsers()
    {
        // Arrange
        var response = new AdminUsersResponse
        {
            Users = new List<AdminUserListDto>
            {
                new() { UserId = 1, Email = "user1@example.com", Role = "Customer", FullName = "User 1", IsActive = true, CreatedAt = DateTime.Now },
                new() { UserId = 2, Email = "user2@example.com", Role = "Admin", FullName = "User 2", IsActive = true, CreatedAt = DateTime.Now }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 50
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAllUsers(1, 50, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualResponse = Assert.IsType<AdminUsersResponse>(okResult.Value);
        Assert.Equal(2, actualResponse.Users.Count);
    }

    [Fact]
    public async Task GetAllUsers_WithSearchTerm_ReturnsFilteredUsers()
    {
        // Arrange
        var response = new AdminUsersResponse
        {
            Users = new List<AdminUserListDto>
            {
                new() { UserId = 1, Email = "john@example.com", Role = "Customer", FullName = "John Doe", IsActive = true, CreatedAt = DateTime.Now }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 50
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetAllUsersQuery>(q => q.SearchTerm == "john"), 
            default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAllUsers(1, 50, "john", null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualResponse = Assert.IsType<AdminUsersResponse>(okResult.Value);
        Assert.Single(actualResponse.Users);
    }

    [Fact]
    public async Task GetAllUsers_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllUsers(1, 50, null, null);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WithValidUserId_ReturnsOkWithUser()
    {
        // Arrange
        var userId = 1;
        var response = new AdminUserDto
        {
            UserId = userId,
            Email = "user@example.com",
            Role = "Customer",
            FullName = "Test User",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetUserByIdQuery>(q => q.UserId == userId),
            default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualResponse = Assert.IsType<AdminUserDto>(okResult.Value);
        Assert.Equal(userId, actualResponse.UserId);
    }

    [Fact]
    public async Task GetUserById_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), default))
            .ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateUserRole_WithValidData_ReturnsOk()
    {
        // Arrange
        var userId = 1;
        var request = new UpdateUserRoleRequest { UserId = userId, Role = "Admin" };
        var response = new AdminUserDto 
        { 
            UserId = userId, 
            Email = "user@example.com", 
            Role = "Admin", 
            FullName = "Test User",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<UpdateUserRoleCommand>(c => c.UserId == userId && c.Role == "Admin"),
            default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateUserRole(userId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdateUserRole_WhenInvalidRole_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var request = new UpdateUserRoleRequest { UserId = userId, Role = "InvalidRole" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default))
            .ThrowsAsync(new Exception("Invalid role"));

        // Act
        var result = await _controller.UpdateUserRole(userId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteUser_WithValidUserId_ReturnsOk()
    {
        // Arrange
        var userId = 1;
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteUserCommand>(c => c.UserId == userId),
            default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteUser_WhenUserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var userId = 999;
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), default))
            .ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}
