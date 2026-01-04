using System.ComponentModel.DataAnnotations;

namespace HealthSync.Application.DTOs;

public class AddFoodEntryDto
{
    [Required]
    public int FoodItemId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required(ErrorMessage = "MealType is required")]
    [RegularExpression("^(Breakfast|Lunch|Dinner|Snack)$", ErrorMessage = "MealType must be one of: Breakfast, Lunch, Dinner, Snack")]
    public string MealType { get; set; } = string.Empty;
}
