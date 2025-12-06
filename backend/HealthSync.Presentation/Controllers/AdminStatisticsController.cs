using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;

namespace HealthSync.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminStatisticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminStatisticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get comprehensive admin statistics for dashboard
        /// </summary>
        /// <param name="days">Number of days to include in statistics (default: 365)</param>
        /// <returns>Admin statistics including user, workout, nutrition, and goal data</returns>
        [HttpGet("statistics")]
        [RequirePermission(PermissionCodes.DASHBOARD_ADMIN)]
        [ProducesResponseType(typeof(AdminStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminStatisticsDto>> GetStatistics([FromQuery] int? days = 365)
        {
            var query = new GetAdminStatisticsQuery { Days = days };
            var statistics = await _mediator.Send(query);
            return Ok(statistics);
        }

        /// <summary>
        /// Get user statistics summary
        /// </summary>
        [HttpGet("statistics/users")]
        [RequirePermission(PermissionCodes.DASHBOARD_ADMIN)]
        [ProducesResponseType(typeof(UserStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics([FromQuery] int? days = 365)
        {
            var query = new GetAdminStatisticsQuery { Days = days };
            var statistics = await _mediator.Send(query);
            return Ok(statistics.UserStatistics);
        }

        /// <summary>
        /// Get workout statistics summary
        /// </summary>
        [HttpGet("statistics/workouts")]
        [RequirePermission(PermissionCodes.DASHBOARD_ADMIN)]
        [ProducesResponseType(typeof(WorkoutStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WorkoutStatisticsDto>> GetWorkoutStatistics([FromQuery] int? days = 365)
        {
            var query = new GetAdminStatisticsQuery { Days = days };
            var statistics = await _mediator.Send(query);
            return Ok(statistics.WorkoutStatistics);
        }

        /// <summary>
        /// Get nutrition statistics summary
        /// </summary>
        [HttpGet("statistics/nutrition")]
        [RequirePermission(PermissionCodes.DASHBOARD_ADMIN)]
        [ProducesResponseType(typeof(NutritionStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NutritionStatisticsDto>> GetNutritionStatistics([FromQuery] int? days = 365)
        {
            var query = new GetAdminStatisticsQuery { Days = days };
            var statistics = await _mediator.Send(query);
            return Ok(statistics.NutritionStatistics);
        }

        /// <summary>
        /// Get goal statistics summary
        /// </summary>
        [HttpGet("statistics/goals")]
        [RequirePermission(PermissionCodes.DASHBOARD_ADMIN)]
        [ProducesResponseType(typeof(GoalStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GoalStatisticsDto>> GetGoalStatistics([FromQuery] int? days = 365)
        {
            var query = new GetAdminStatisticsQuery { Days = days };
            var statistics = await _mediator.Send(query);
            return Ok(statistics.GoalStatistics);
        }
    }
}
