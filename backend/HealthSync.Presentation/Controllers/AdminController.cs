using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using HealthSync.Presentation.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;
    private readonly IMinioClient _minioClient;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AdminController(IMediator mediator, ILogger<AdminController> logger, IMinioClient minioClient, HttpClient httpClient, IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _minioClient = minioClient;
        _httpClient = httpClient;
        _configuration = configuration;
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

    [HttpPost("users")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var command = new CreateUserCommand
            {
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role,
                Password = request.Password
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("users/{userId}")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var command = new UpdateUserCommand
            {
                UserId = userId,
                FullName = request.FullName,
                Role = request.Role
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("users/{userId}/avatar")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> UpdateUserAvatar(int userId, [FromForm] UpdateAvatarFormRequest request)
    {
        try
        {
            // Tìm user
            var query = new GetUserByIdQuery { UserId = userId };
            var user = await _mediator.Send(query);
            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            string avatarUrl;

            if (request.File != null)
            {
                // Upload file
                var command = new UploadAvatarCommand
                {
                    UserId = userId,
                    FileStream = request.File.OpenReadStream(),
                    FileName = request.File.FileName,
                    ContentType = request.File.ContentType
                };
                avatarUrl = await _mediator.Send(command);
            }
            else
            {
                // Generate từ DiceBear
                var publicUrl = _configuration["MinIO:PublicUrl"] ?? "http://localhost:9002";
                var avatarSeeder = new AvatarSeeder(_minioClient, _httpClient, publicUrl);
                avatarUrl = await avatarSeeder.SeedAvatarAsync(user.UserName);
                if (avatarUrl == null)
                {
                    return StatusCode(500, new { Error = "Failed to generate avatar" });
                }
            }

            return Ok(new { Message = "Avatar updated successfully", AvatarUrl = avatarUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user avatar: {UserId}", userId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpDelete("users/{userId}")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = userId };
            var result = await _mediator.Send(command);
            return Ok(new { Message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("users/{userId}/password")]
    [RequirePermission(PermissionCodes.USER_UPDATE_ROLE)]
    public async Task<IActionResult> UpdateUserPassword(int userId, [FromBody] UpdatePasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { Error = "Mật khẩu mới không được để trống" });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { Error = "Mật khẩu phải có ít nhất 6 ký tự" });
            }

            var command = new UpdateUserPasswordCommand
            {
                UserId = userId,
                NewPassword = request.NewPassword
            };

            var result = await _mediator.Send(command);
            
            if (!result)
            {
                return NotFound(new { Error = "Không tìm thấy người dùng" });
            }

            return Ok(new { Message = "Cập nhật mật khẩu thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user password: {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public class UpdatePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateAvatarRequest
{
    public string? Url { get; set; }
}

public class UpdateAvatarFormRequest
{
    public IFormFile? File { get; set; }
}
