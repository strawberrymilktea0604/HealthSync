namespace HealthSync.Domain.Entities;

public class FoodEntry
{
    public Guid FoodEntryId { get; set; }
    public Guid NutritionLogId { get; set; }
    public Guid FoodItemId { get; set; }
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