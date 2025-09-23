namespace HealthSync.Domain.Entities;

public class NutritionLog
{
    public Guid NutritionLogId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LogDate { get; set; }
    public decimal? TotalCalories { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<FoodEntry> FoodEntries { get; set; } = new List<FoodEntry>();
}