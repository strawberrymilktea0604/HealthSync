using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class CreateFoodItemCommandHandler : IRequestHandler<CreateFoodItemCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateFoodItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateFoodItemCommand request, CancellationToken cancellationToken)
    {
        var foodItem = new FoodItem
        {
            Name = request.Name,
            ServingSize = request.ServingSize,
            ServingUnit = request.ServingUnit,
            CaloriesKcal = request.CaloriesKcal,
            ProteinG = request.ProteinG,
            CarbsG = request.CarbsG,
            FatG = request.FatG
        };

        _context.Add(foodItem);
        await _context.SaveChangesAsync(cancellationToken);

        return foodItem.FoodItemId;
    }
}
