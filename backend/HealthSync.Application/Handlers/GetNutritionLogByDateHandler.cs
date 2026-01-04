using HealthSync.Application.DTOs;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Queries;

public class GetNutritionLogByDateHandler : IRequestHandler<GetNutritionLogByDateQuery, NutritionLogDto?>
{
    private readonly IApplicationDbContext _context;

    public GetNutritionLogByDateHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NutritionLogDto?> Handle(GetNutritionLogByDateQuery request, CancellationToken cancellationToken)
    {
        var nutritionLog = await _context.NutritionLogs
            .Include(nl => nl.FoodEntries)
                .ThenInclude(fe => fe.FoodItem)
            .FirstOrDefaultAsync(nl => nl.UserId == request.UserId && nl.LogDate.Date == request.Date.Date, cancellationToken);

        if (nutritionLog == null)
        {
            return null;
        }

        return new NutritionLogDto
        {
            NutritionLogId = nutritionLog.NutritionLogId,
            UserId = nutritionLog.UserId,
            LogDate = nutritionLog.LogDate,
            TotalCalories = nutritionLog.TotalCalories,
            ProteinG = nutritionLog.ProteinG,
            CarbsG = nutritionLog.CarbsG,
            FatG = nutritionLog.FatG,
            FoodEntries = nutritionLog.FoodEntries.Select(fe => new FoodEntryDto
            {
                FoodEntryId = fe.FoodEntryId,
                FoodItemId = fe.FoodItemId,
                FoodItemName = fe.FoodItem.Name,
                Quantity = fe.Quantity,
                MealType = fe.MealType,
                CaloriesKcal = fe.CaloriesKcal ?? 0,
                ProteinG = fe.ProteinG ?? 0,
                CarbsG = fe.CarbsG ?? 0,
                FatG = fe.FatG ?? 0,
                ImageUrl = fe.FoodItem.ImageUrl
            }).ToList()
        };
    }
}
