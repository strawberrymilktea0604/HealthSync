using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("users")]
    [RequirePermission(PermissionCodes.USER_READ)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null)
    {
        try
        {
            var query = new GetAllUsersQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Role = role
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("users/{userId}")]
    [RequirePermission(PermissionCodes.USER_READ)]
    public async Task<IActionResult> GetUserById(int userId)
    {
        try
        {
            var query = new GetUserByIdQuery { UserId = userId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpPut("users/{userId}/role")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var command = new UpdateUserRoleCommand
            {
                UserId = userId,
                Role = request.Role
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role: {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("users/{userId}")]
    [RequirePermission(PermissionCodes.USER_DELETE)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = userId };
            var result = await _mediator.Send(command);
            return Ok(new { Success = result, Message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }
}
