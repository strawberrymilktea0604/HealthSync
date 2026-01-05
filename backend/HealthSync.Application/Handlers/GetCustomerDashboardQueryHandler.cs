using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
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
                throw new UnauthorizedAccessException("User account no longer exists or has been deleted.");
            }

            // Get user info
            var userInfo = new UserInfoDto
            {
                UserId = user.UserId,
                FullName = user.Profile?.FullName ?? user.Email,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl ?? user.Profile?.AvatarUrl ?? "" // Prioritize ApplicationUser.AvatarUrl
            };

            // Get all active goals (in_progress or active status)
            var activeGoals = user.Goals
                .Where(g => g.Status == "in_progress" || g.Status == "active")
                .OrderByDescending(g => g.GoalId)
                .ToList();

            // Get the primary goal (latest one) for detailed progress
            var primaryGoal = activeGoals.FirstOrDefault();

            GoalProgressDto? goalProgress = null;
            WeightProgressDto? weightProgress = null;
            var activeGoalsSummary = new List<GoalSummaryDto>();

            // Create summary for all active goals using helper method
            foreach (var goal in activeGoals)
            {
                activeGoalsSummary.Add(CreateGoalSummary(goal));
            }

            if (primaryGoal != null)
            {
                (goalProgress, weightProgress) = CalculatePrimaryGoalProgress(primaryGoal, user.Profile?.WeightKg ?? 0);
            }

            // Get today's stats
            // Assuming simplified logic: use Server time offset by 7 hours for Vietnam
            var today = DateTime.UtcNow.AddHours(7).Date;
            int caloriesConsumed = 0;
            var todayNutritionLogs = await _context.NutritionLogs
                .Include(n => n.FoodEntries)
                .Where(n => n.UserId == request.UserId && n.LogDate.Date == today)
                .ToListAsync(cancellationToken);

            if (todayNutritionLogs.Any())
            {
                caloriesConsumed = (int)todayNutritionLogs
                    .SelectMany(n => n.FoodEntries)
                    .Sum(f => f.CaloriesKcal ?? 0);
            }

            // Calculate weekly workout minutes (Start of week is Monday)
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (today.DayOfWeek == DayOfWeek.Sunday) startOfWeek = startOfWeek.AddDays(-7); // Fix Sunday being start of week in some locales

            var weeklyWorkoutLogs = await _context.WorkoutLogs
                .Where(w => w.UserId == request.UserId && w.WorkoutDate.Date >= startOfWeek && w.WorkoutDate.Date <= today)
                .ToListAsync(cancellationToken);

            int workoutMinutes = weeklyWorkoutLogs.Sum(w => w.DurationMin);

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
                ActiveGoals = activeGoalsSummary,
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

        private static GoalSummaryDto CreateGoalSummary(Goal goal)
        {
            var records = goal.ProgressRecords.OrderBy(p => p.RecordDate).ToList();
            var firstRecord = records.FirstOrDefault();
            var latestRecord = records.LastOrDefault();
            
            decimal startVal = firstRecord?.WeightKg ?? firstRecord?.Value ?? 0;
            decimal currentVal = latestRecord?.WeightKg ?? latestRecord?.Value ?? startVal;
            decimal targetVal = goal.TargetValue;
            
            decimal progressPct = CalculateProgressPercentage(startVal, currentVal, targetVal, goal.Type);
            
            return new GoalSummaryDto
            {
                GoalId = goal.GoalId,
                Type = goal.Type,
                Notes = goal.Notes ?? "",
                TargetValue = goal.TargetValue,
                Progress = progressPct
            };
        }

        private static (GoalProgressDto, WeightProgressDto) CalculatePrimaryGoalProgress(Goal primaryGoal, decimal defaultWeight)
        {
            var sortedRecords = primaryGoal.ProgressRecords.OrderBy(p => p.RecordDate).ToList();
            var firstProgress = sortedRecords.FirstOrDefault();
            var latestProgress = sortedRecords.LastOrDefault();

            decimal startValue = firstProgress?.WeightKg ?? firstProgress?.Value ?? defaultWeight;
            decimal currentValue = latestProgress?.WeightKg ?? latestProgress?.Value ?? startValue;
            decimal targetValue = primaryGoal.TargetValue;
            
            bool isDecrease = IsDecreaseGoal(primaryGoal.Type, startValue, targetValue);
            
            decimal progressAmount = isDecrease ? startValue - currentValue : currentValue - startValue;
            decimal remaining = isDecrease ? currentValue - targetValue : targetValue - currentValue;
            decimal progressPercentage = CalculateProgressPercentage(startValue, currentValue, targetValue, primaryGoal.Type);

            var goalProgress = new GoalProgressDto
            {
                GoalType = primaryGoal.Type,
                StartValue = startValue,
                CurrentValue = currentValue,
                TargetValue = targetValue,
                Status = primaryGoal.Status,
                Progress = progressPercentage,
                ProgressAmount = progressAmount,
                Remaining = remaining
            };

            var weightHistory = primaryGoal.ProgressRecords
                .OrderBy(p => p.RecordDate)
                .Select(p => new WeightDataPointDto { Date = p.RecordDate, Weight = p.WeightKg })
                .ToList();

            var daysRemaining = primaryGoal.EndDate.HasValue ? (primaryGoal.EndDate.Value - DateTime.UtcNow).Days : 0;
            var weightProgress = new WeightProgressDto
            {
                WeightHistory = weightHistory,
                TimeRemaining = daysRemaining > 0 ? $"{daysRemaining} days" : "N/A"
            };

            return (goalProgress, weightProgress);
        }

        private static bool IsDecreaseGoal(string goalType, decimal startValue, decimal targetValue)
        {
            return goalType.ToLower().Contains("loss") || 
                   goalType.ToLower().Contains("giảm") ||
                   targetValue < startValue;
        }

        private static decimal CalculateProgressPercentage(decimal startValue, decimal currentValue, decimal targetValue, string goalType)
        {
            bool isDecrease = IsDecreaseGoal(goalType, startValue, targetValue);
            decimal totalChange = isDecrease ? startValue - targetValue : targetValue - startValue;
            
            if (totalChange <= 0) return 0;
            
            decimal progress = isDecrease ? startValue - currentValue : currentValue - startValue;
            decimal percentage = (progress / totalChange) * 100;
            return Math.Max(0, Math.Min(100, percentage));
        }
    }
}
