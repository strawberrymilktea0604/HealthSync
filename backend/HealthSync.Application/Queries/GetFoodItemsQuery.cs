using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetFoodItemsQuery : IRequest<List<FoodItemDto>>
{
    public string? Search { get; set; }
}