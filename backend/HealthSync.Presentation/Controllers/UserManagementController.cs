using HealthSync.Application.Commands;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Presentation.Controllers;

/// <summary>
/// Admin controller for managing user roles and permissions
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize] // Must be authenticated
public class UserManagementController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserManagementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Assign a role to a user (Admin only)
    /// </summary>
    [HttpPost("{userId}/roles/{roleId}")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> AssignRole(int userId, int roleId)
    {
        var command = new AssignRoleToUserCommand
        {
            UserId = userId,
            RoleId = roleId
        };

        var result = await _mediator.Send(command);

        if (result)
            return Ok(new { message = "Role assigned successfully" });

        return BadRequest(new { message = "Role already assigned or invalid request" });
    }

    /// <summary>
    /// Remove a role from a user (Admin only)
    /// </summary>
    [HttpDelete("{userId}/roles/{roleId}")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> RemoveRole(int userId, int roleId)
    {
        var command = new RemoveRoleFromUserCommand
        {
            UserId = userId,
            RoleId = roleId
        };

        var result = await _mediator.Send(command);

        if (result)
            return Ok(new { message = "Role removed successfully" });

        return NotFound(new { message = "Role assignment not found" });
    }

    /// <summary>
    /// Get all roles assigned to a user (Admin only)
    /// </summary>
    [HttpGet("{userId}/roles")]
    [RequirePermission(PermissionCodes.USER_READ)]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        var query = new GetUserRolesQuery { UserId = userId };
        var roles = await _mediator.Send(query);

        return Ok(new { userId, roles });
    }

    /// <summary>
    /// Get all permissions a user has (Admin only)
    /// </summary>
    [HttpGet("{userId}/permissions")]
    [RequirePermission(PermissionCodes.USER_READ)]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var query = new GetUserPermissionsQuery { UserId = userId };
        var permissions = await _mediator.Send(query);

        return Ok(new { userId, permissions });
    }
}
