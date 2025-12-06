namespace HealthSync.Application.DTOs
{
    public class CustomerDashboardDto
    {
        public required UserInfoDto UserInfo { get; set; }
        public required GoalProgressDto GoalProgress { get; set; }
        public required WeightProgressDto WeightProgress { get; set; }
        public required TodayStatsDto TodayStats { get; set; }
        public required ExerciseStreakDto ExerciseStreak { get; set; }
    }

    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class GoalProgressDto
    {
        public required string GoalType { get; set; }
        public decimal StartValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal Progress { get; set; }
        public decimal Remaining { get; set; }
        public required string Status { get; set; }
    }

    public class WeightProgressDto
    {
        public decimal CurrentWeight { get; set; }
        public decimal TargetWeight { get; set; }
        public decimal WeightLost { get; set; }
        public decimal WeightRemaining { get; set; }
        public decimal ProgressPercentage { get; set; }
        public required List<WeightDataPointDto> WeightHistory { get; set; }
        public int DaysRemaining { get; set; }
        public required string TimeRemaining { get; set; }
    }

    public class WeightDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal Weight { get; set; }
    }

    public class TodayStatsDto
    {
        public int CaloriesConsumed { get; set; }
        public int CaloriesTarget { get; set; }
        public int WorkoutMinutes { get; set; }
        public required string WorkoutDuration { get; set; }
    }

    public class ExerciseStreakDto
    {
        public int CurrentStreak { get; set; }
        public int TotalDays { get; set; }
    }
}
