using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg
            };

            var userId = await _mediator.Send(command);
            return Created($"/api/users/{userId}", new { UserId = userId, Message = "Đăng ký thành công!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var query = new LoginQuery
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Error = "Sai email hoặc mật khẩu!" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }
}