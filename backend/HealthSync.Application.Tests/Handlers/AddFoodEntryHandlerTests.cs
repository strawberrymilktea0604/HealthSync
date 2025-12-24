using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class AddFoodEntryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly AddFoodEntryHandler _handler;

    public AddFoodEntryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new AddFoodEntryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewNutritionLog_WhenLogDoesNotExist()
    {
        // Arrange
        var nutritionLogs = new List<NutritionLog>();
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Chicken Breast",
            CaloriesKcal = 165m,
            ProteinG = 31m,
            CarbsG = 0m,
            FatG = 3.6m
        };
        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        NutritionLog? capturedLog = null;
        _contextMock.Setup(c => c.Add(It.IsAny<NutritionLog>()))
            .Callback<object>(obj => capturedLog = obj as NutritionLog);

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (capturedLog != null)
                {
                    capturedLog.NutritionLogId = 1;
                    nutritionLogs.Add(capturedLog);
                }
            })
            .ReturnsAsync(1);

        var command = new AddFoodEntryCommand
        {
            UserId = 1,
            FoodItemId = 1,
            Quantity = 1.5m,
            MealType = "Lunch",
            LogDate = new DateTime(2025, 12, 24)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(1, capturedLog.UserId);
        Assert.Equal(new DateTime(2025, 12, 24).Date, capturedLog.LogDate);
        _contextMock.Verify(c => c.Add(It.IsAny<NutritionLog>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseExistingNutritionLog_WhenLogExists()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = new DateTime(2025, 12, 24).Date,
            TotalCalories = 500m,
            ProteinG = 50m,
            CarbsG = 40m,
            FatG = 20m,
            FoodEntries = new List<FoodEntry>()
        };
        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Banana",
            CaloriesKcal = 89m,
            ProteinG = 1.1m,
            CarbsG = 23m,
            FatG = 0.3m
        };
        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AddFoodEntryCommand
        {
            UserId = 1,
            FoodItemId = 1,
            Quantity = 2m,
            MealType = "Snack",
            LogDate = new DateTime(2025, 12, 24)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.Add(It.IsAny<NutritionLog>()), Times.Never); // Should not create new log
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenFoodItemNotFound()
    {
        // Arrange
        var nutritionLogs = new List<NutritionLog>();
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var foodItems = new List<FoodItem>();
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new AddFoodEntryCommand
        {
            UserId = 1,
            FoodItemId = 999,
            Quantity = 1m,
            MealType = "Lunch",
            LogDate = DateTime.Now
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Food item not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldCalculateNutritionValues_BasedOnQuantity()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Now.Date,
            TotalCalories = 0,
            ProteinG = 0,
            CarbsG = 0,
            FatG = 0,
            FoodEntries = new List<FoodEntry>()
        };
        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Rice",
            CaloriesKcal = 130m,
            ProteinG = 2.7m,
            CarbsG = 28m,
            FatG = 0.3m
        };
        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        FoodEntry? capturedEntry = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodEntry>()))
            .Callback<object>(obj => capturedEntry = obj as FoodEntry);

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AddFoodEntryCommand
        {
            UserId = 1,
            FoodItemId = 1,
            Quantity = 2m, // 2x serving
            MealType = "Dinner",
            LogDate = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal(260m, capturedEntry.CaloriesKcal); // 130 * 2
        Assert.Equal(5.4m, capturedEntry.ProteinG); // 2.7 * 2
        Assert.Equal(56m, capturedEntry.CarbsG); // 28 * 2
        Assert.Equal(0.6m, capturedEntry.FatG); // 0.3 * 2
    }

    [Fact]
    public async Task Handle_ShouldUpdateNutritionLogTotals()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Now.Date,
            TotalCalories = 500m,
            ProteinG = 30m,
            CarbsG = 50m,
            FatG = 15m,
            FoodEntries = new List<FoodEntry>()
        };
        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Egg",
            CaloriesKcal = 78m,
            ProteinG = 6.3m,
            CarbsG = 0.6m,
            FatG = 5.3m
        };
        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AddFoodEntryCommand
        {
            UserId = 1,
            FoodItemId = 1,
            Quantity = 1m,
            MealType = "Breakfast",
            LogDate = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(578m, nutritionLog.TotalCalories); // 500 + 78
        Assert.Equal(36.3m, nutritionLog.ProteinG); // 30 + 6.3
        Assert.Equal(50.6m, nutritionLog.CarbsG); // 50 + 0.6
        Assert.Equal(20.3m, nutritionLog.FatG); // 15 + 5.3
    }

    [Fact]
    public async Task Handle_ShouldReturnFoodEntryId()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Now.Date,
            TotalCalories = 0,
            ProteinG = 0,
            CarbsG = 0,
            FatG = 0,
            FoodEntries = new List<FoodEntry>()
        };
        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            CaloriesKcal = 100m,
            ProteinG = 10m,
            CarbsG = 20m,
            FatG = 5m
        };
        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        FoodEntry? capturedEntry = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodEntry>()))
            .Callback<object>(obj =>
            {
                capturedEntry = obj as FoodEntry;
                if (capturedEntry != null)
                    capturedEntry.FoodEntryId = 42;
            });

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AddFoodEntryCommand
        {
            UserId = 1,
            FoodItemId = 1,
            Quantity = 1m,
            MealType = "Lunch",
            LogDate = DateTime.Now
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(42, result);
    }
}
