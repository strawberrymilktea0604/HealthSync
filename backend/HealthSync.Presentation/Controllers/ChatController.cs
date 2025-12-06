using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using System.Security.Claims;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Yêu cầu đăng nhập
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMediator mediator, ILogger<ChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Chat với AI HealthBot - Chỉ Customer được sử dụng
    /// </summary>
    [HttpPost("ask")]
    [Authorize(Roles = "Customer,Admin")] // Cả Customer và Admin đều có thể dùng
    public async Task<ActionResult<ChatResponseDto>> AskHealthBot([FromBody] ChatRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { message = "Question cannot be empty" });
            }

            var query = new ChatWithBotQuery
            {
                UserId = userId,
                Question = request.Question
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, new { message = "Đã có lỗi xảy ra. Vui lòng thử lại sau." });
        }
    }

    /// <summary>
    /// Lấy lịch sử chat của user
    /// </summary>
    [HttpGet("history")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<List<ChatHistoryDto>>> GetChatHistory(
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var query = new GetChatHistoryQuery
            {
                UserId = userId,
                PageSize = Math.Min(pageSize, 50), // Giới hạn tối đa 50
                PageNumber = Math.Max(pageNumber, 1)
            };

            var history = await _mediator.Send(query);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            return StatusCode(500, new { message = "Không thể tải lịch sử chat" });
        }
    }

    /// <summary>
    /// Health check endpoint cho chatbot service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "HealthSync Chatbot",
            timestamp = DateTime.UtcNow
        });
    }
}
