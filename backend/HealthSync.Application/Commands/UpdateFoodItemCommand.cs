using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateFoodItemCommand : IRequest<bool>
{
    public int FoodItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ServingSize { get; set; }
    public string ServingUnit { get; set; } = "g";
    public decimal CaloriesKcal { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}
