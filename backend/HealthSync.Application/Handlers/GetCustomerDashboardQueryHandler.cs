using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers
{
    public class GetCustomerDashboardQueryHandler : IRequestHandler<GetCustomerDashboardQuery, CustomerDashboardDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerDashboardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDashboardDto> Handle(GetCustomerDashboardQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.ApplicationUsers
                .Include(u => u.Profile)
                .Include(u => u.Goals)
                    .ThenInclude(g => g.ProgressRecords)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Get user info
            var userInfo = new UserInfoDto
            {
                UserId = user.UserId,
                FullName = user.Profile?.FullName ?? user.Email,
                Email = user.Email,
                AvatarUrl = (user.Profile?.AvatarUrl ?? user.AvatarUrl) ?? ""
            };

            // Get active goal
            var activeGoal = user.Goals
                .Where(g => g.Status == "active")
                .OrderByDescending(g => g.GoalId)
                .FirstOrDefault();

            GoalProgressDto? goalProgress = null;
            WeightProgressDto? weightProgress = null;

            if (activeGoal != null)
            {
                var latestProgress = activeGoal.ProgressRecords
                    .OrderByDescending(p => p.RecordDate)
                    .FirstOrDefault();

                decimal currentValue = latestProgress?.Value ?? activeGoal.TargetValue; // Assuming start with target or something, but need to adjust
                decimal progress = 0;
                decimal remaining = 0;

                if (activeGoal.Type.ToLower().Contains("loss") || activeGoal.Type.ToLower().Contains("giảm"))
                {
                    progress = (user.Profile?.WeightKg ?? 0) - currentValue; // Use current weight
                    remaining = currentValue - activeGoal.TargetValue;
                }
                else
                {
                    progress = currentValue - (user.Profile?.WeightKg ?? 0);
                    remaining = activeGoal.TargetValue - currentValue;
                }

                goalProgress = new GoalProgressDto
                {
                    GoalType = activeGoal.Type,
                    Status = activeGoal.Status,
                    Progress = progress,
                    Remaining = remaining
                };

                // Weight progress with chart data
                var weightHistory = activeGoal.ProgressRecords
                    .OrderBy(p => p.RecordDate)
                    .Select(p => new WeightDataPointDto
                    {
                        Date = p.RecordDate,
                        Weight = p.WeightKg
                    })
                    .ToList();

                var daysRemaining = activeGoal.EndDate.HasValue ? (activeGoal.EndDate.Value - DateTime.UtcNow).Days : 0;

                weightProgress = new WeightProgressDto
                {
                    WeightHistory = weightHistory,
                    TimeRemaining = daysRemaining > 0 ? $"{daysRemaining} days" : "N/A"
                };
            }

            // Get today's stats
            // Assuming simplified logic: use Server time offset by 7 hours for Vietnam
            var today = DateTime.UtcNow.AddHours(7).Date;
            var todayNutritionLog = await _context.NutritionLogs
                .Include(n => n.FoodEntries)
                .Where(n => n.UserId == request.UserId && n.LogDate.Date == today)
                .FirstOrDefaultAsync(cancellationToken);

            var todayWorkoutLog = await _context.WorkoutLogs
                .Include(w => w.ExerciseSessions)
                .Where(w => w.UserId == request.UserId && w.WorkoutDate.Date == today)
                .FirstOrDefaultAsync(cancellationToken);

            int caloriesConsumed = 0;
            if (todayNutritionLog != null && todayNutritionLog.FoodEntries != null)
            {
                caloriesConsumed = (int)(todayNutritionLog.FoodEntries.Sum(f => f.CaloriesKcal ?? 0));
            }

            int workoutMinutes = 0;
            if (todayWorkoutLog != null)
            {
                workoutMinutes = todayWorkoutLog.DurationMin;
            }

            var todayStats = new TodayStatsDto
            {
                WorkoutDuration = $"{workoutMinutes} min",
                CaloriesConsumed = caloriesConsumed
            };

            // Calculate exercise streak
            var workoutDates = await _context.WorkoutLogs
                .Where(w => w.UserId == request.UserId)
                .OrderByDescending(w => w.WorkoutDate)
                .Select(w => w.WorkoutDate.Date)
                .Distinct()
                .ToListAsync(cancellationToken);

            int currentStreak = CalculateStreak(workoutDates);

            var exerciseStreak = new ExerciseStreakDto
            {
                CurrentStreak = currentStreak
            };

            return new CustomerDashboardDto
            {
                UserInfo = userInfo,
                GoalProgress = goalProgress ?? new GoalProgressDto { GoalType = "None", Status = "No active goal" },
                WeightProgress = weightProgress ?? new WeightProgressDto { WeightHistory = new List<WeightDataPointDto>(), TimeRemaining = "N/A" },
                TodayStats = todayStats,
                ExerciseStreak = exerciseStreak
            };
        }

        private static int CalculateStreak(List<DateTime> workoutDates)
        {
            if (workoutDates.Count == 0) return 0;

            int streak = 0;
            var today = DateTime.UtcNow.Date;
            var checkDate = today;

            foreach (var date in workoutDates)
            {
                if (date == checkDate || date == checkDate.AddDays(-1))
                {
                    streak++;
                    checkDate = date.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }
}
