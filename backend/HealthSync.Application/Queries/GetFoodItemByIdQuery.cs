using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetFoodItemByIdQuery : IRequest<FoodItemDto?>
{
    public int FoodItemId { get; set; }
}
