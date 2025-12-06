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

    [HttpGet("nutrition-log")]
    public async Task<IActionResult> GetNutritionLogByDate([FromQuery] DateTime date)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var query = new GetNutritionLogByDateQuery
        {
            UserId = userId,
            Date = date
        };

        var nutritionLog = await _mediator.Send(query);
        return Ok(nutritionLog);
    }

    [HttpGet("nutrition-logs")]
    public async Task<IActionResult> GetNutritionLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        // Get UserId from JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
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

    [HttpPost("food-entry")]
    public async Task<IActionResult> AddFoodEntry([FromBody] AddFoodEntryDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var command = new AddFoodEntryCommand
        {
            UserId = userId,
            LogDate = DateTime.UtcNow,
            FoodItemId = dto.FoodItemId,
            Quantity = dto.Quantity,
            MealType = dto.MealType
        };

        var foodEntryId = await _mediator.Send(command);
        return Ok(new { FoodEntryId = foodEntryId });
    }

    [HttpDelete("food-entry/{id}")]
    public async Task<IActionResult> DeleteFoodEntry(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var command = new DeleteFoodEntryCommand
        {
            FoodEntryId = id,
            UserId = userId
        };

        var result = await _mediator.Send(command);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("nutrition-logs")]
    public async Task<IActionResult> CreateNutritionLog([FromBody] CreateNutritionLogDto dto)
    {
        // Get UserId from JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
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