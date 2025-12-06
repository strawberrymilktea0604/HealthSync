using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthSync.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("summary")]
        [RequirePermission(PermissionCodes.DASHBOARD_ADMIN)]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            var query = new GetDashboardSummaryQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("customer")]
        [RequirePermission(PermissionCodes.DASHBOARD_VIEW)]
        public async Task<ActionResult<CustomerDashboardDto>> GetCustomerDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var query = new GetCustomerDashboardQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}