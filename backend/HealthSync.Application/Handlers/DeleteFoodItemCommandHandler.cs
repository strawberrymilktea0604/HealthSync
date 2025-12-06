using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class DeleteFoodItemCommandHandler : IRequestHandler<DeleteFoodItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteFoodItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFoodItemCommand request, CancellationToken cancellationToken)
    {
        var foodItem = await _context.FoodItems
            .FirstOrDefaultAsync(f => f.FoodItemId == request.FoodItemId, cancellationToken);

        if (foodItem == null)
        {
            return false;
        }

        // Check if food item is being used in any food entries
        var isUsed = await _context.FoodEntries
            .AnyAsync(fe => fe.FoodItemId == request.FoodItemId, cancellationToken);

        if (isUsed)
        {
            throw new InvalidOperationException("Không thể xóa món ăn đang được sử dụng trong nhật ký dinh dưỡng.");
        }

        _context.Remove(foodItem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
