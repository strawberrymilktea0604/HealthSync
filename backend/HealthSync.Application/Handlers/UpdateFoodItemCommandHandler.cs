using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class UpdateFoodItemCommandHandler : IRequestHandler<UpdateFoodItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateFoodItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateFoodItemCommand request, CancellationToken cancellationToken)
    {
        var foodItem = await _context.FoodItems
            .FirstOrDefaultAsync(f => f.FoodItemId == request.FoodItemId, cancellationToken);

        if (foodItem == null)
        {
            return false;
        }

        foodItem.Name = request.Name;
        foodItem.ServingSize = request.ServingSize;
        foodItem.ServingUnit = request.ServingUnit;
        foodItem.CaloriesKcal = request.CaloriesKcal;
        foodItem.ProteinG = request.ProteinG;
        foodItem.CarbsG = request.CarbsG;
        foodItem.FatG = request.FatG;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
