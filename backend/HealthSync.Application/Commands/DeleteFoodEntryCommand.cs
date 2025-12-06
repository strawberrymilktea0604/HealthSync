using MediatR;

namespace HealthSync.Application.Commands;

public class DeleteFoodEntryCommand : IRequest<bool>
{
    public int FoodEntryId { get; set; }
    public int UserId { get; set; }
}
