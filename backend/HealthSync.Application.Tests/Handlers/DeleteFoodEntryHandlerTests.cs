using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class DeleteFoodEntryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteFoodEntryHandler _handler;

    public DeleteFoodEntryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteFoodEntryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenFoodEntryIsDeleted()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            TotalCalories = 500m,
            ProteinG = 30m,
            CarbsG = 50m,
            FatG = 15m
        };

        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            NutritionLogId = 1,
            FoodItemId = 1,
            Quantity = 1m,
            CaloriesKcal = 200m,
            ProteinG = 10m,
            CarbsG = 20m,
            FatG = 5m,
            NutritionLog = nutritionLog
        };

        var foodEntries = new List<FoodEntry> { foodEntry };
        var mockFoodEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockFoodEntries);
        _contextMock.Setup(c => c.Remove(It.IsAny<FoodEntry>()));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteFoodEntryCommand { FoodEntryId = 1, UserId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Remove(foodEntry), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenFoodEntryNotFound()
    {
        // Arrange
        var foodEntries = new List<FoodEntry>();
        var mockFoodEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockFoodEntries);

        var command = new DeleteFoodEntryCommand { FoodEntryId = 999, UserId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<FoodEntry>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserIdDoesNotMatch()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 2, // Different user
            TotalCalories = 500m,
            ProteinG = 30m,
            CarbsG = 50m,
            FatG = 15m
        };

        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            NutritionLogId = 1,
            CaloriesKcal = 200m,
            ProteinG = 10m,
            CarbsG = 20m,
            FatG = 5m,
            NutritionLog = nutritionLog
        };

        var foodEntries = new List<FoodEntry> { foodEntry };
        var mockFoodEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockFoodEntries);

        var command = new DeleteFoodEntryCommand { FoodEntryId = 1, UserId = 1 }; // User 1 trying to delete User 2's entry

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<FoodEntry>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateNutritionLogTotals_WhenDeleting()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            TotalCalories = 500m,
            ProteinG = 30m,
            CarbsG = 50m,
            FatG = 15m
        };

        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            NutritionLogId = 1,
            CaloriesKcal = 200m,
            ProteinG = 10m,
            CarbsG = 20m,
            FatG = 5m,
            NutritionLog = nutritionLog
        };

        var foodEntries = new List<FoodEntry> { foodEntry };
        var mockFoodEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockFoodEntries);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteFoodEntryCommand { FoodEntryId = 1, UserId = 1 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(300m, nutritionLog.TotalCalories); // 500 - 200
        Assert.Equal(20m, nutritionLog.ProteinG); // 30 - 10
        Assert.Equal(30m, nutritionLog.CarbsG); // 50 - 20
        Assert.Equal(10m, nutritionLog.FatG); // 15 - 5
    }

    [Fact]
    public async Task Handle_ShouldHandleNullNutrientValues()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            TotalCalories = 500m,
            ProteinG = 30m,
            CarbsG = 50m,
            FatG = 15m
        };

        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            NutritionLogId = 1,
            CaloriesKcal = null,
            ProteinG = null,
            CarbsG = null,
            FatG = null,
            NutritionLog = nutritionLog
        };

        var foodEntries = new List<FoodEntry> { foodEntry };
        var mockFoodEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockFoodEntries);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteFoodEntryCommand { FoodEntryId = 1, UserId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500m, nutritionLog.TotalCalories); // No change (null treated as 0)
        Assert.Equal(30m, nutritionLog.ProteinG);
        Assert.Equal(50m, nutritionLog.CarbsG);
        Assert.Equal(15m, nutritionLog.FatG);
    }

    [Fact]
    public async Task Handle_ShouldSubtractCorrectValues_WithDecimalQuantities()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            TotalCalories = 825m,
            ProteinG = 46.5m,
            CarbsG = 50m,
            FatG = 18.9m
        };

        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            NutritionLogId = 1,
            CaloriesKcal = 247.5m, // 165 * 1.5
            ProteinG = 46.5m, // 31 * 1.5
            CarbsG = 0m,
            FatG = 5.4m, // 3.6 * 1.5
            NutritionLog = nutritionLog
        };

        var foodEntries = new List<FoodEntry> { foodEntry };
        var mockFoodEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockFoodEntries);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteFoodEntryCommand { FoodEntryId = 1, UserId = 1 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(577.5m, nutritionLog.TotalCalories);
        Assert.Equal(0m, nutritionLog.ProteinG);
        Assert.Equal(50m, nutritionLog.CarbsG);
        Assert.Equal(13.5m, nutritionLog.FatG);
    }
}
