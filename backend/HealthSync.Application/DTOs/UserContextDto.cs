namespace HealthSync.Application.DTOs;

public class UserContextDto
{
    public ProfileContextDto Profile { get; set; } = new();
    public GoalContextDto? Goal { get; set; }
    public List<DailyLogContextDto> RecentLogsLast7Days { get; set; } = new();
    
    // NEW: Lịch sử thao tác gần đây từ Data Warehouse
    public string? RecentActivityLogs { get; set; }

    // NEW: Danh sách tài nguyên hệ thống (để gợi ý)
    public List<string> AvailableExercisesSummary { get; set; } = new();
    public List<string> AvailableFoodsSummary { get; set; } = new();
    
    // NEW: Danh sách các mục tiêu khác (ví dụ đã hoàn thành)
    public List<string> CompletedGoalsHistory { get; set; } = new();
}

public class ProfileContextDto
{
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal HeightCm { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public decimal Bmr { get; set; }
    public decimal Bmi { get; set; }
    public string BmiStatus { get; set; } = string.Empty; // "Underweight", "Normal", "Overweight", "Obese"
    public string ActivityLevel { get; set; } = string.Empty;
}

public class GoalContextDto
{
    public string Type { get; set; } = string.Empty;
    public decimal TargetWeightKg { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal CurrentProgress { get; set; }
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
    public List<string> FoodItems { get; set; } = new(); // NEW: Chi tiết món ăn
}

public class WorkoutContextDto
{
    public string Status { get; set; } = "Rest";
    public int? DurationMin { get; set; }
    public List<string> Focus { get; set; } = new();
    public string? Notes { get; set; }
    public List<string> Exercises { get; set; } = new(); // NEW: Chi tiết bài tập
}
