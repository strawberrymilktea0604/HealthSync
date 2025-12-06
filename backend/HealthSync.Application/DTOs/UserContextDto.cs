namespace HealthSync.Application.DTOs;

public class UserContextDto
{
    public ProfileContextDto Profile { get; set; } = new();
    public GoalContextDto? Goal { get; set; }
    public List<DailyLogContextDto> RecentLogsLast7Days { get; set; } = new();
}

public class ProfileContextDto
{
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal HeightCm { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public decimal Bmr { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
}

public class GoalContextDto
{
    public string Type { get; set; } = string.Empty;
    public decimal TargetWeightKg { get; set; }
    public DateTime? Deadline { get; set; }
}

public class DailyLogContextDto
{
    public DateTime Date { get; set; }
    public NutritionContextDto? Nutrition { get; set; }
    public WorkoutContextDto? Workout { get; set; }
}

public class NutritionContextDto
{
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
    public string? Notes { get; set; }
}

public class WorkoutContextDto
{
    public string Status { get; set; } = "Rest";
    public int? DurationMin { get; set; }
    public List<string> Focus { get; set; } = new();
    public string? Notes { get; set; }
}
