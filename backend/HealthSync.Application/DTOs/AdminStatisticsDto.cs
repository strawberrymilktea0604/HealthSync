namespace HealthSync.Application.DTOs
{
    public class AdminStatisticsDto
    {
        public required UserStatisticsDto UserStatistics { get; set; }
        public required WorkoutStatisticsDto WorkoutStatistics { get; set; }
        public required NutritionStatisticsDto NutritionStatistics { get; set; }
        public required GoalStatisticsDto GoalStatistics { get; set; }
    }

    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisWeek { get; set; }
        public required List<UserGrowthDto> UserGrowthData { get; set; }
        public required List<UserRoleDistributionDto> UserRoleDistribution { get; set; }
    }

    public class UserGrowthDto
    {
        public required string Period { get; set; } // "2024-01", "2024-02", etc.
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class UserRoleDistributionDto
    {
        public required string Role { get; set; }
        public int Count { get; set; }
    }

    public class WorkoutStatisticsDto
    {
        public int TotalWorkoutLogs { get; set; }
        public int WorkoutLogsThisMonth { get; set; }
        public int TotalExercises { get; set; }
        public required List<PopularExerciseDto> TopExercises { get; set; }
        public required List<WorkoutActivityDto> WorkoutActivityData { get; set; }
        public required List<MuscleGroupDistributionDto> MuscleGroupDistribution { get; set; }
    }

    public class PopularExerciseDto
    {
        public int ExerciseId { get; set; }
        public required string ExerciseName { get; set; }
        public required string MuscleGroup { get; set; }
        public int UsageCount { get; set; }
    }

    public class WorkoutActivityDto
    {
        public required string Period { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class MuscleGroupDistributionDto
    {
        public required string MuscleGroup { get; set; }
        public int Count { get; set; }
    }

    public class NutritionStatisticsDto
    {
        public int TotalNutritionLogs { get; set; }
        public int NutritionLogsThisMonth { get; set; }
        public int TotalFoodItems { get; set; }
        public required List<PopularFoodDto> TopFoods { get; set; }
        public required List<NutritionActivityDto> NutritionActivityData { get; set; }
        public required AverageNutritionDto AverageDailyNutrition { get; set; }
    }

    public class PopularFoodDto
    {
        public int FoodItemId { get; set; }
        public required string FoodName { get; set; }
        public int UsageCount { get; set; }
        public decimal Calories { get; set; }
    }

    public class NutritionActivityDto
    {
        public required string Period { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class AverageNutritionDto
    {
        public decimal AverageCalories { get; set; }
        public decimal AverageProtein { get; set; }
        public decimal AverageCarbs { get; set; }
        public decimal AverageFat { get; set; }
    }

    public class GoalStatisticsDto
    {
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public required List<GoalTypeDistributionDto> GoalTypeDistribution { get; set; }
        public required List<GoalStatusDistributionDto> GoalStatusDistribution { get; set; }
        public decimal GoalCompletionRate { get; set; }
    }

    public class GoalTypeDistributionDto
    {
        public required string GoalType { get; set; }
        public int Count { get; set; }
    }

    public class GoalStatusDistributionDto
    {
        public required string Status { get; set; }
        public int Count { get; set; }
    }
}
