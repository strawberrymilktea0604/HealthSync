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
public class WorkoutController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("exercises")]
    public async Task<IActionResult> GetExercises(
        [FromQuery] string? muscleGroup,
        [FromQuery] string? difficulty,
        [FromQuery] string? search)
    {
        var query = new GetExercisesQuery
        {
            MuscleGroup = muscleGroup,
            Difficulty = difficulty,
            Search = search
        };

        var exercises = await _mediator.Send(query);
        return Ok(exercises);
    }

    [HttpGet("workout-logs")]
    public async Task<IActionResult> GetWorkoutLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        // Get UserId from JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var query = new GetWorkoutLogsQuery
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate
        };

        var workoutLogs = await _mediator.Send(query);
        return Ok(workoutLogs);
    }

    [HttpPost("workout-logs")]
    public async Task<IActionResult> CreateWorkoutLog([FromBody] CreateWorkoutLogDto dto)
    {
        // Get UserId from JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var command = new CreateWorkoutLogCommand
        {
            UserId = userId,
            WorkoutLog = dto
        };

        var workoutLogId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetWorkoutLogs), new { id = workoutLogId }, new { WorkoutLogId = workoutLogId });
    }
}