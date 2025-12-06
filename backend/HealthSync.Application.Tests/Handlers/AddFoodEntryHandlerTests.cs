using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class AddFoodEntryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly AddFoodEntryHandler _handler;

    public AddFoodEntryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new AddFoodEntryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidFoodEntry_CreatesFoodEntrySuccessfully()
    {
        // Arrange
        var command = new AddFoodEntryCommand
        {
            NutritionLogId = 1,
            FoodEntry = new AddFoodEntryDto
            {
                FoodItemId = 10,
                Quantity = 2.0m,
                MealType = "Breakfast",
                CaloriesKcal = 500,
                ProteinG = 25,
                CarbsG = 50,
                FatG = 15
            }
        };

        FoodEntry? capturedFoodEntry = null;
        _mockContext.Setup(x => x.Add(It.IsAny<FoodEntry>()))
            .Callback<FoodEntry>(fe => capturedFoodEntry = fe);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { if (capturedFoodEntry != null) capturedFoodEntry.FoodEntryId = 123; })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(123, result);
        Assert.NotNull(capturedFoodEntry);
        Assert.Equal(1, capturedFoodEntry.NutritionLogId);
        Assert.Equal(10, capturedFoodEntry.FoodItemId);
        Assert.Equal(2.0m, capturedFoodEntry.Quantity);
        Assert.Equal("Breakfast", capturedFoodEntry.MealType);
        
        _mockContext.Verify(x => x.Add(It.IsAny<FoodEntry>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Breakfast", 300, 20, 30, 10)]
    [InlineData("Lunch", 600, 35, 70, 20)]
    [InlineData("Dinner", 500, 30, 50, 18)]
    [InlineData("Snack", 150, 5, 20, 7)]
    public async Task Handle_DifferentMealTypes_CreatesCorrectly(
        string mealType,
        decimal calories,
        decimal protein,
        decimal carbs,
        decimal fat)
    {
        // Arrange
        var command = new AddFoodEntryCommand
        {
            NutritionLogId = 1,
            FoodEntry = new AddFoodEntryDto
            {
                FoodItemId = 5,
                Quantity = 1.0m,
                MealType = mealType,
                CaloriesKcal = calories,
                ProteinG = protein,
                CarbsG = carbs,
                FatG = fat
            }
        };

        FoodEntry? capturedFoodEntry = null;
        _mockContext.Setup(x => x.Add(It.IsAny<FoodEntry>()))
            .Callback<FoodEntry>(fe => capturedFoodEntry = fe);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedFoodEntry);
        Assert.Equal(mealType, capturedFoodEntry.MealType);
        Assert.Equal(calories, capturedFoodEntry.CaloriesKcal);
        Assert.Equal(protein, capturedFoodEntry.ProteinG);
        Assert.Equal(carbs, capturedFoodEntry.CarbsG);
        Assert.Equal(fat, capturedFoodEntry.FatG);
    }

    [Fact]
    public async Task Handle_MultipleServings_CalculatesNutritionCorrectly()
    {
        // Arrange
        var command = new AddFoodEntryCommand
        {
            NutritionLogId = 2,
            FoodEntry = new AddFoodEntryDto
            {
                FoodItemId = 15,
                Quantity = 3.5m, // 3.5 servings
                MealType = "Lunch",
                CaloriesKcal = 700, // Total for 3.5 servings
                ProteinG = 35,
                CarbsG = 70,
                FatG = 21
            }
        };

        FoodEntry? capturedFoodEntry = null;
        _mockContext.Setup(x => x.Add(It.IsAny<FoodEntry>()))
            .Callback<FoodEntry>(fe => capturedFoodEntry = fe);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedFoodEntry);
        Assert.Equal(3.5m, capturedFoodEntry.Quantity);
        Assert.Equal(700, capturedFoodEntry.CaloriesKcal);
    }
}
