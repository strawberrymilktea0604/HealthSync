using MediatR;

namespace HealthSync.Application.Commands;

public class DeleteFoodItemCommand : IRequest<bool>
{
    public int FoodItemId { get; set; }
}
