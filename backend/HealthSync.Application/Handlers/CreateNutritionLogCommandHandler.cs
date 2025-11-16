using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class CreateNutritionLogCommandHandler : IRequestHandler<CreateNutritionLogCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateNutritionLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateNutritionLogCommand request, CancellationToken cancellationToken)
    {
        var nutritionLog = new NutritionLog
        {
            UserId = request.UserId,
            LogDate = request.NutritionLog.LogDate,
            FoodEntries = request.NutritionLog.FoodEntries.Select(fe => new FoodEntry
            {
                FoodItemId = fe.FoodItemId,
                Quantity = fe.Quantity,
                MealType = fe.MealType
                // Calories, Protein, etc. will be calculated based on FoodItem
            }).ToList()
        };

        // Calculate totals for each FoodEntry
        foreach (var entry in nutritionLog.FoodEntries)
        {
            var foodItem = await _context.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == entry.FoodItemId);
            if (foodItem != null)
            {
                var factor = entry.Quantity / foodItem.ServingSize;
                entry.CaloriesKcal = foodItem.CaloriesKcal * factor;
                entry.ProteinG = foodItem.ProteinG * factor;
                entry.CarbsG = foodItem.CarbsG * factor;
                entry.FatG = foodItem.FatG * factor;
            }
        }

        // Calculate total calories for the log
        nutritionLog.TotalCalories = nutritionLog.FoodEntries.Sum(fe => fe.CaloriesKcal ?? 0);

        _context.Add(nutritionLog);
        await _context.SaveChangesAsync(cancellationToken);

        return nutritionLog.NutritionLogId;
    }
}