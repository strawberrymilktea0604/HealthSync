using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NutritionController : ControllerBase
{
    private readonly IMediator _mediator;

    public NutritionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("food-items")]
    public async Task<IActionResult> GetFoodItems([FromQuery] string? search)
    {
        var query = new GetFoodItemsQuery
        {
            Search = search
        };

        var foodItems = await _mediator.Send(query);
        return Ok(foodItems);
    }

    [HttpGet("nutrition-logs")]
    public async Task<IActionResult> GetNutritionLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        // Get UserId from JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var query = new GetNutritionLogsQuery
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate
        };

        var nutritionLogs = await _mediator.Send(query);
        return Ok(nutritionLogs);
    }

    [HttpPost("nutrition-logs")]
    public async Task<IActionResult> CreateNutritionLog([FromBody] CreateNutritionLogDto dto)
    {
        // Get UserId from JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new CreateNutritionLogCommand
        {
            UserId = userId,
            NutritionLog = dto
        };

        var nutritionLogId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetNutritionLogs), new { id = nutritionLogId }, new { NutritionLogId = nutritionLogId });
    }
}