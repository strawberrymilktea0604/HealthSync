using FluentValidation.TestHelper;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Validators;

namespace HealthSync.Application.Tests.Validators;

public class CreateNutritionLogCommandValidatorTests
{
    private readonly CreateNutritionLogCommandValidator _validator;

    public CreateNutritionLogCommandValidatorTests()
    {
        _validator = new CreateNutritionLogCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now.AddHours(-1),
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 100,
                        MealType = "Breakfast"
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_UserIdZero_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 0,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto { FoodItemId = 1, Quantity = 100, MealType = "Breakfast" }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be greater than 0.");
    }

    [Fact]
    public void Validate_LogDateInFuture_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now.AddDays(1),
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto { FoodItemId = 1, Quantity = 100, MealType = "Breakfast" }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NutritionLog.LogDate)
            .WithErrorMessage("LogDate cannot be in the future.");
    }

    [Fact]
    public void Validate_EmptyFoodEntries_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>()
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NutritionLog.FoodEntries)
            .WithErrorMessage("At least one food entry is required.");
    }

    [Fact]
    public void Validate_FoodEntryWithZeroQuantity_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 0,
                        MealType = "Breakfast"
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("NutritionLog.FoodEntries[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");
    }

    [Fact]
    public void Validate_FoodEntryWithNegativeQuantity_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = -10,
                        MealType = "Breakfast"
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("NutritionLog.FoodEntries[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");
    }

    [Fact]
    public void Validate_FoodEntryWithExcessiveQuantity_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 15000, // Exceeds 10000
                        MealType = "Breakfast"
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("NutritionLog.FoodEntries[0].Quantity")
            .WithErrorMessage("Quantity cannot exceed 10000.");
    }

    [Fact]
    public void Validate_FoodEntryWithEmptyMealType_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 100,
                        MealType = ""
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("NutritionLog.FoodEntries[0].MealType")
            .WithErrorMessage("MealType is required.");
    }

    [Fact]
    public void Validate_FoodEntryWithInvalidMealType_FailsValidation()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 100,
                        MealType = "Midnight Snack"
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("NutritionLog.FoodEntries[0].MealType")
            .WithErrorMessage("MealType must be one of: Breakfast, Lunch, Dinner, Snack.");
    }

    [Theory]
    [InlineData("Breakfast")]
    [InlineData("Lunch")]
    [InlineData("Dinner")]
    [InlineData("Snack")]
    public void Validate_FoodEntryWithValidMealTypes_PassesValidation(string mealType)
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now.AddHours(-1),
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 100,
                        MealType = mealType
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MultipleFoodEntries_ValidatesAll()
    {
        // Arrange
        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = DateTime.Now.AddHours(-1),
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 100,
                        MealType = "Breakfast"
                    },
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 2,
                        Quantity = 150,
                        MealType = "Lunch"
                    },
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 3,
                        Quantity = 200,
                        MealType = "Dinner"
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
