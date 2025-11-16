namespace HealthSync.Domain.Entities;

public class NutritionLog
{
    public int NutritionLogId { get; set; }
    public int UserId { get; set; }
    public DateTime LogDate { get; set; }
    public decimal TotalCalories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<FoodEntry> FoodEntries { get; set; } = new List<FoodEntry>();
}