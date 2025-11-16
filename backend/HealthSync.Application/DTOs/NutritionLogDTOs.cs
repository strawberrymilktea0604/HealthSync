namespace HealthSync.Application.DTOs;

public class NutritionLogDto
{
    public int NutritionLogId { get; set; }
    public int UserId { get; set; }
    public DateTime LogDate { get; set; }
    public decimal TotalCalories { get; set; }
    public List<FoodEntryDto> FoodEntries { get; set; } = new List<FoodEntryDto>();
}

public class CreateNutritionLogDto
{
    public DateTime LogDate { get; set; }
    public List<CreateFoodEntryDto> FoodEntries { get; set; } = new List<CreateFoodEntryDto>();
}

public class FoodEntryDto
{
    public int FoodEntryId { get; set; }
    public int FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string MealType { get; set; } = string.Empty;
    public decimal CaloriesKcal { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}

public class CreateFoodEntryDto
{
    public int FoodItemId { get; set; }
    public decimal Quantity { get; set; }
    public string MealType { get; set; } = string.Empty;
}