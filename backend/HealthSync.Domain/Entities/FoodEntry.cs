namespace HealthSync.Domain.Entities;

public class FoodEntry
{
    public int FoodEntryId { get; set; }
    public int NutritionLogId { get; set; }
    public int FoodItemId { get; set; }
    public decimal Quantity { get; set; }
    public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
    public decimal? CaloriesKcal { get; set; }
    public decimal? ProteinG { get; set; }
    public decimal? CarbsG { get; set; }
    public decimal? FatG { get; set; }

    // Navigation properties
    public NutritionLog NutritionLog { get; set; } = null!;
    public FoodItem FoodItem { get; set; } = null!;
}