using FluentValidation;
using HealthSync.Application.Commands;

namespace HealthSync.Application.Validators;

public class CreateNutritionLogCommandValidator : AbstractValidator<CreateNutritionLogCommand>
{
    public CreateNutritionLogCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.NutritionLog.LogDate)
            .NotEmpty().WithMessage("LogDate is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("LogDate cannot be in the future.");

        RuleFor(x => x.NutritionLog.FoodEntries)
            .NotEmpty().WithMessage("At least one food entry is required.");

        RuleForEach(x => x.NutritionLog.FoodEntries)
            .SetValidator(new CreateFoodEntryDtoValidator());
    }
}

public class CreateFoodEntryDtoValidator : AbstractValidator<DTOs.CreateFoodEntryDto>
{
    public CreateFoodEntryDtoValidator()
    {
        RuleFor(x => x.FoodItemId)
            .GreaterThan(0).WithMessage("FoodItemId must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10000.");

        RuleFor(x => x.MealType)
            .NotEmpty().WithMessage("MealType is required.")
            .Must(mt => new[] { "Breakfast", "Lunch", "Dinner", "Snack" }.Contains(mt))
            .WithMessage("MealType must be one of: Breakfast, Lunch, Dinner, Snack.");
    }
}