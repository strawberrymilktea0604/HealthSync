using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GoalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var command = new CreateGoalCommand
        {
            UserId = userId.Value,
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetGoals), new { id = result.GoalId }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetGoalsQuery
        {
            UserId = userId.Value
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/progress")]
    public async Task<IActionResult> AddProgress(int id, [FromBody] AddProgressRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var command = new AddProgressCommand
        {
            GoalId = id,
            UserId = userId.Value,
            RecordDate = request.RecordDate,
            Value = request.Value,
            Notes = request.Notes,
            WeightKg = request.WeightKg,
            WaistCm = request.WaistCm
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}