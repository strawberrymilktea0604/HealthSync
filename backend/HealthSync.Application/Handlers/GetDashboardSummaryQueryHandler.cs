using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers
{
    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalUsers = await _context.ApplicationUsers.CountAsync(cancellationToken);
            var newUsersThisMonth = await _context.ApplicationUsers
                .Where(u => u.CreatedAt >= firstDayOfMonth)
                .CountAsync(cancellationToken);
            var totalWorkoutLogs = await _context.WorkoutLogs.CountAsync(cancellationToken);
            var totalNutritionLogs = await _context.NutritionLogs.CountAsync(cancellationToken);
            var totalGoals = await _context.Goals.CountAsync(cancellationToken);
            var activeGoals = await _context.Goals
                .Where(g => g.Status == "active")
                .CountAsync(cancellationToken);

            return new DashboardSummaryDto
            {
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,
                TotalWorkoutLogs = totalWorkoutLogs,
                TotalNutritionLogs = totalNutritionLogs,
                TotalGoals = totalGoals,
                ActiveGoals = activeGoals
            };
        }
    }
}