using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Commands;

public class AddFoodEntryHandler : IRequestHandler<AddFoodEntryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddFoodEntryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddFoodEntryCommand request, CancellationToken cancellationToken)
    {
        // Get or create nutrition log for the date
        var nutritionLog = await _context.NutritionLogs
            .Include(nl => nl.FoodEntries)
            .FirstOrDefaultAsync(nl => nl.UserId == request.UserId && nl.LogDate.Date == request.LogDate.Date, cancellationToken);

        if (nutritionLog == null)
        {
            nutritionLog = new NutritionLog
            {
                UserId = request.UserId,
                LogDate = request.LogDate.Date,
                TotalCalories = 0,
                ProteinG = 0,
                CarbsG = 0,
                FatG = 0
            };
            _context.Add(nutritionLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Get food item details
        var foodItem = await _context.FoodItems
            .FirstOrDefaultAsync(f => f.FoodItemId == request.FoodItemId, cancellationToken);

        if (foodItem == null)
        {
            throw new Exception("Food item not found");
        }

        // Calculate nutritional values based on quantity
        var calories = foodItem.CaloriesKcal * request.Quantity;
        var protein = foodItem.ProteinG * request.Quantity;
        var carbs = foodItem.CarbsG * request.Quantity;
        var fat = foodItem.FatG * request.Quantity;

        // Create food entry
        var foodEntry = new FoodEntry
        {
            NutritionLogId = nutritionLog.NutritionLogId,
            FoodItemId = request.FoodItemId,
            Quantity = request.Quantity,
            MealType = request.MealType,
            CaloriesKcal = calories,
            ProteinG = protein,
            CarbsG = carbs,
            FatG = fat
        };

        _context.Add(foodEntry);

        // Update nutrition log totals
        nutritionLog.TotalCalories += calories;
        nutritionLog.ProteinG += protein;
        nutritionLog.CarbsG += carbs;
        nutritionLog.FatG += fat;

        await _context.SaveChangesAsync(cancellationToken);

        return foodEntry.FoodEntryId;
    }
}
