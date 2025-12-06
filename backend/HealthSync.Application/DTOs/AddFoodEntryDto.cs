namespace HealthSync.Application.DTOs;

public class AddFoodEntryDto
{
    public int FoodItemId { get; set; }
    public decimal Quantity { get; set; }
    public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
}
