using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetFoodItemByIdQueryHandler : IRequestHandler<GetFoodItemByIdQuery, FoodItemDto?>
{
    private readonly IApplicationDbContext _context;

    public GetFoodItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FoodItemDto?> Handle(GetFoodItemByIdQuery request, CancellationToken cancellationToken)
    {
        var foodItem = await _context.FoodItems
            .Where(f => f.FoodItemId == request.FoodItemId)
            .Select(f => new FoodItemDto
            {
                FoodItemId = f.FoodItemId,
                Name = f.Name,
                ServingSize = f.ServingSize,
                ServingUnit = f.ServingUnit,
                CaloriesKcal = f.CaloriesKcal,
                ProteinG = f.ProteinG,
                CarbsG = f.CarbsG,
                FatG = f.FatG,
                ImageUrl = f.ImageUrl // FIX: Thêm mapping ImageUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        return foodItem;
    }
}
