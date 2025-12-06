using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Commands;

public class DeleteFoodEntryHandler : IRequestHandler<DeleteFoodEntryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteFoodEntryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFoodEntryCommand request, CancellationToken cancellationToken)
    {
        var foodEntry = await _context.FoodEntries
            .Include(fe => fe.NutritionLog)
            .FirstOrDefaultAsync(fe => fe.FoodEntryId == request.FoodEntryId && fe.NutritionLog.UserId == request.UserId, cancellationToken);

        if (foodEntry == null)
        {
            return false;
        }

        // Update nutrition log totals
        var nutritionLog = foodEntry.NutritionLog;
        nutritionLog.TotalCalories -= foodEntry.CaloriesKcal ?? 0;
        nutritionLog.ProteinG -= foodEntry.ProteinG ?? 0;
        nutritionLog.CarbsG -= foodEntry.CarbsG ?? 0;
        nutritionLog.FatG -= foodEntry.FatG ?? 0;

        _context.Remove(foodEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
