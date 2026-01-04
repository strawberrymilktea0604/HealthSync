using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetNutritionLogsQueryHandler : IRequestHandler<GetNutritionLogsQuery, List<NutritionLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNutritionLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NutritionLogDto>> Handle(GetNutritionLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NutritionLogs
            .Where(n => n.UserId == request.UserId)
            .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(n => n.LogDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(n => n.LogDate <= request.EndDate.Value);
        }

        var nutritionLogs = await query
            .Include(n => n.FoodEntries)
            .ThenInclude(fe => fe.FoodItem)
            .Select(n => new NutritionLogDto
            {
                NutritionLogId = n.NutritionLogId,
                UserId = n.UserId,
                LogDate = n.LogDate,
                TotalCalories = n.TotalCalories,
                ProteinG = n.FoodEntries.Sum(fe => fe.ProteinG ?? 0),
                CarbsG = n.FoodEntries.Sum(fe => fe.CarbsG ?? 0),
                FatG = n.FoodEntries.Sum(fe => fe.FatG ?? 0),
                FoodEntries = n.FoodEntries.Select(fe => new FoodEntryDto
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
            })
            .ToListAsync(cancellationToken);

        return nutritionLogs;
    }
}