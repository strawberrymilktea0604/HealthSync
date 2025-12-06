using MediatR;

namespace HealthSync.Application.Commands;

public class AddFoodEntryCommand : IRequest<int>
{
    public int UserId { get; set; }
    public DateTime LogDate { get; set; }
    public int FoodItemId { get; set; }
    public decimal Quantity { get; set; }
    public string MealType { get; set; } = string.Empty;
}
