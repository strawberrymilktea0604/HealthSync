using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class GetFoodItemsQueryHandler : IRequestHandler<GetFoodItemsQuery, List<FoodItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFoodItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FoodItemDto>> Handle(GetFoodItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FoodItems.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(f => f.Name.Contains(request.Search));
        }

        var foodItems = await query
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
            .ToListAsync(cancellationToken);

        return foodItems;
    }
}