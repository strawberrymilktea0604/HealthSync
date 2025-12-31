using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class UpdateFoodItemImageCommandHandler : IRequestHandler<UpdateFoodItemImageCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateFoodItemImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateFoodItemImageCommand request, CancellationToken cancellationToken)
    {
        var foodItem = await _context.FoodItems
            .FirstOrDefaultAsync(f => f.FoodItemId == request.FoodItemId, cancellationToken);

        if (foodItem == null)
        {
            throw new InvalidOperationException("Không tìm thấy món ăn");
        }

        foodItem.ImageUrl = request.ImageUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
