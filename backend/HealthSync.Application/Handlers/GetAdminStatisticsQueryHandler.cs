using MediatR;
using Microsoft.EntityFrameworkCore;
using HealthSync.Application.DTOs;
using HealthSync.Application.Extensions;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;

namespace HealthSync.Application.Handlers
{
    public class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminStatisticsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken cancellationToken)
        {
            var days = request.Days ?? 365; // Default to 1 year
            var startDate = DateTime.UtcNow.AddDays(-days);

            try
            {
                var statistics = new AdminStatisticsDto
                {
                    UserStatistics = await GetUserStatistics(startDate, cancellationToken),
                    WorkoutStatistics = await GetWorkoutStatistics(startDate, cancellationToken),
                    NutritionStatistics = await GetNutritionStatistics(startDate, cancellationToken),
                    GoalStatistics = await GetGoalStatistics(cancellationToken)
                };

                return statistics;
            }
            catch (Exception ex)
            {
                // Log the exception and return empty statistics to prevent crashes
                Console.WriteLine($"Error in GetAdminStatisticsQueryHandler: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return empty statistics instead of throwing
                return new AdminStatisticsDto
                {
                    UserStatistics = new UserStatisticsDto
                    {
                        TotalUsers = 0,
                        ActiveUsers = 0,
                        NewUsersThisWeek = 0,
                        NewUsersThisMonth = 0,
                        UserGrowthData = new List<UserGrowthDto>(),
                        UserRoleDistribution = new List<UserRoleDistributionDto>()
                    },
                    WorkoutStatistics = new WorkoutStatisticsDto
                    {
                        TotalWorkoutLogs = 0,
                        WorkoutLogsThisMonth = 0,
                        TotalExercises = 0,
                        TopExercises = new List<PopularExerciseDto>(),
                        WorkoutActivityData = new List<WorkoutActivityDto>(),
                        MuscleGroupDistribution = new List<MuscleGroupDistributionDto>()
                    },
                    NutritionStatistics = new NutritionStatisticsDto
                    {
                        TotalNutritionLogs = 0,
                        NutritionLogsThisMonth = 0,
                        TotalFoodItems = 0,
                        TopFoods = new List<PopularFoodDto>(),
                        NutritionActivityData = new List<NutritionActivityDto>(),
                        AverageDailyNutrition = new AverageNutritionDto
                        {
                            AverageCalories = 0,
                            AverageProtein = 0,
                            AverageCarbs = 0,
                            AverageFat = 0
                        }
                    },
                    GoalStatistics = new GoalStatisticsDto
                    {
                        TotalGoals = 0,
                        ActiveGoals = 0,
                        CompletedGoals = 0,
                        GoalTypeDistribution = new List<GoalTypeDistributionDto>(),
                        GoalStatusDistribution = new List<GoalStatusDistributionDto>(),
                        GoalCompletionRate = 0
                    }
                };
            }
        }

        private async Task<UserStatisticsDto> GetUserStatistics(DateTime startDate, CancellationToken cancellationToken)
        {
            var users = await _context.ApplicationUsers
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);
            var totalUsers = users.Count;
            var activeUsers = users.Count(u => u.IsActive);

            var now = DateTime.UtcNow;
            var oneWeekAgo = now.AddDays(-7);
            var oneMonthAgo = now.AddMonths(-1);

            var newUsersThisWeek = users.Count(u => u.CreatedAt >= oneWeekAgo);
            var newUsersThisMonth = users.Count(u => u.CreatedAt >= oneMonthAgo);

            // User growth data (monthly)
            var userGrowthData = users
                .Where(u => u.CreatedAt >= startDate)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new UserGrowthDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // User role distribution
            var userRoleDistribution = users
                .GroupBy(u => u.GetRoleName())
                .Select(g => new UserRoleDistributionDto
                {
                    Role = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return new UserStatisticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsersThisWeek = newUsersThisWeek,
                NewUsersThisMonth = newUsersThisMonth,
                UserGrowthData = userGrowthData,
                UserRoleDistribution = userRoleDistribution
            };
        }

        private async Task<WorkoutStatisticsDto> GetWorkoutStatistics(DateTime startDate, CancellationToken cancellationToken)
        {
            var workoutLogs = await _context.WorkoutLogs
                .Include(w => w.ExerciseSessions)
                .ThenInclude(es => es.Exercise)
                .ToListAsync(cancellationToken);

            var totalWorkoutLogs = workoutLogs.Count;
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var workoutLogsThisMonth = workoutLogs.Count(w => w.WorkoutDate >= oneMonthAgo);

            var totalExercises = await _context.Exercises.CountAsync(cancellationToken);

            // Top exercises
            var topExercises = workoutLogs
                .SelectMany(w => w.ExerciseSessions)
                .Where(es => es.Exercise != null && !string.IsNullOrEmpty(es.Exercise.Name) && !string.IsNullOrEmpty(es.Exercise.MuscleGroup))
                .GroupBy(es => new { es.ExerciseId, es.Exercise.Name, es.Exercise.MuscleGroup })
                .Select(g => new PopularExerciseDto
                {
                    ExerciseId = g.Key.ExerciseId,
                    ExerciseName = g.Key.Name,
                    MuscleGroup = g.Key.MuscleGroup,
                    UsageCount = g.Count()
                })
                .OrderByDescending(x => x.UsageCount)
                .Take(10)
                .ToList();

            // Workout activity data
            var workoutActivityData = workoutLogs
                .Where(w => w.WorkoutDate >= startDate)
                .GroupBy(w => new { w.WorkoutDate.Year, w.WorkoutDate.Month })
                .Select(g => new WorkoutActivityDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Muscle group distribution
            var muscleGroupDistribution = workoutLogs
                .SelectMany(w => w.ExerciseSessions)
                .Where(es => es.Exercise != null && !string.IsNullOrEmpty(es.Exercise.MuscleGroup))
                .GroupBy(es => es.Exercise.MuscleGroup)
                .Select(g => new MuscleGroupDistributionDto
                {
                    MuscleGroup = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new WorkoutStatisticsDto
            {
                TotalWorkoutLogs = totalWorkoutLogs,
                WorkoutLogsThisMonth = workoutLogsThisMonth,
                TotalExercises = totalExercises,
                TopExercises = topExercises,
                WorkoutActivityData = workoutActivityData,
                MuscleGroupDistribution = muscleGroupDistribution
            };
        }

        private async Task<NutritionStatisticsDto> GetNutritionStatistics(DateTime startDate, CancellationToken cancellationToken)
        {
            var nutritionLogs = await _context.NutritionLogs
                .Include(n => n.FoodEntries)
                .ThenInclude(fe => fe.FoodItem)
                .ToListAsync(cancellationToken);

            var totalNutritionLogs = nutritionLogs.Count;
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var nutritionLogsThisMonth = nutritionLogs.Count(n => n.LogDate >= oneMonthAgo);

            var totalFoodItems = await _context.FoodItems.CountAsync(cancellationToken);

            // Top foods
            var topFoods = nutritionLogs
                .SelectMany(n => n.FoodEntries)
                .Where(fe => fe.FoodItem != null)
                .GroupBy(fe => new { fe.FoodItemId, fe.FoodItem.Name, fe.FoodItem.CaloriesKcal })
                .Select(g => new PopularFoodDto
                {
                    FoodItemId = g.Key.FoodItemId,
                    FoodName = g.Key.Name,
                    UsageCount = g.Count(),
                    Calories = g.Key.CaloriesKcal
                })
                .OrderByDescending(x => x.UsageCount)
                .Take(10)
                .ToList();

            // Nutrition activity data
            var nutritionActivityData = nutritionLogs
                .Where(n => n.LogDate >= startDate)
                .GroupBy(n => new { n.LogDate.Year, n.LogDate.Month })
                .Select(g => new NutritionActivityDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Average daily nutrition
            var allFoodEntries = nutritionLogs.SelectMany(n => n.FoodEntries).ToList();
            var avgCalories = allFoodEntries.Any() ? allFoodEntries.Average(fe => fe.CaloriesKcal ?? 0) : 0;
            var avgProtein = allFoodEntries.Any() ? allFoodEntries.Average(fe => fe.ProteinG ?? 0) : 0;
            var avgCarbs = allFoodEntries.Any() ? allFoodEntries.Average(fe => fe.CarbsG ?? 0) : 0;
            var avgFat = allFoodEntries.Any() ? allFoodEntries.Average(fe => fe.FatG ?? 0) : 0;

            return new NutritionStatisticsDto
            {
                TotalNutritionLogs = totalNutritionLogs,
                NutritionLogsThisMonth = nutritionLogsThisMonth,
                TotalFoodItems = totalFoodItems,
                TopFoods = topFoods,
                NutritionActivityData = nutritionActivityData,
                AverageDailyNutrition = new AverageNutritionDto
                {
                    AverageCalories = avgCalories,
                    AverageProtein = avgProtein,
                    AverageCarbs = avgCarbs,
                    AverageFat = avgFat
                }
            };
        }

        private async Task<GoalStatisticsDto> GetGoalStatistics(CancellationToken cancellationToken)
        {
            var goals = await _context.Goals.ToListAsync(cancellationToken);

            var totalGoals = goals.Count;
            var activeGoals = goals.Count(g => g.Status == "InProgress" || g.Status == "Active");
            var completedGoals = goals.Count(g => g.Status == "Completed");

            // Goal type distribution
            var goalTypeDistribution = goals
                .GroupBy(g => g.Type)
                .Select(g => new GoalTypeDistributionDto
                {
                    GoalType = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Goal status distribution
            var goalStatusDistribution = goals
                .GroupBy(g => g.Status)
                .Select(g => new GoalStatusDistributionDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Goal completion rate
            var completionRate = totalGoals > 0 ? (decimal)completedGoals / totalGoals * 100 : 0;

            return new GoalStatisticsDto
            {
                TotalGoals = totalGoals,
                ActiveGoals = activeGoals,
                CompletedGoals = completedGoals,
                GoalTypeDistribution = goalTypeDistribution,
                GoalStatusDistribution = goalStatusDistribution,
                GoalCompletionRate = Math.Round(completionRate, 2)
            };
        }
    }
}
