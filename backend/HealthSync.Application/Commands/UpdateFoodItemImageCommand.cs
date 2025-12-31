using MediatR;

namespace HealthSync.Application.Commands;

public class UpdateFoodItemImageCommand : IRequest<Unit>
{
    public int FoodItemId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
