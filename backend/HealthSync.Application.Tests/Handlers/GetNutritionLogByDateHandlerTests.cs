using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetNutritionLogByDateHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetNutritionLogByDateHandler _handler;

    public GetNutritionLogByDateHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetNutritionLogByDateHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenLogNotFound()
    {
        // Arrange
        var nutritionLogs = new List<NutritionLog>();
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery { UserId = 1, Date = DateTime.Today };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnLog_WhenLogExists()
    {
        // Arrange
        var foodItem = new FoodItem { FoodItemId = 1, Name = "Apple" };
        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            FoodItemId = 1,
            FoodItem = foodItem,
            Quantity = 1,
            MealType = "Breakfast",
            CaloriesKcal = 95,
            ProteinG = 0.5m,
            CarbsG = 25,
            FatG = 0.3m
        };

        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Today,
            TotalCalories = 95,
            ProteinG = 0.5m,
            CarbsG = 25,
            FatG = 0.3m,
            FoodEntries = new List<FoodEntry> { foodEntry }
        };

        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery { UserId = 1, Date = DateTime.Today };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.NutritionLogId);
        Assert.Equal(95, result.TotalCalories);
        Assert.Single(result.FoodEntries);
        Assert.Equal("Apple", result.FoodEntries[0].FoodItemName);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserIdDoesNotMatch()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 2, // Different user
            LogDate = DateTime.Today,
            TotalCalories = 100,
            FoodEntries = new List<FoodEntry>()
        };

        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery { UserId = 1, Date = DateTime.Today };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenDateDoesNotMatch()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Today.AddDays(-1), // Different date
            TotalCalories = 100,
            FoodEntries = new List<FoodEntry>()
        };

        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery { UserId = 1, Date = DateTime.Today };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldMapFoodEntriesCorrectly()
    {
        // Arrange
        var foodItem1 = new FoodItem { FoodItemId = 1, Name = "Apple" };
        var foodItem2 = new FoodItem { FoodItemId = 2, Name = "Banana" };
        
        var foodEntry1 = new FoodEntry
        {
            FoodEntryId = 1,
            FoodItemId = 1,
            FoodItem = foodItem1,
            Quantity = 2,
            MealType = "Breakfast",
            CaloriesKcal = 190,
            ProteinG = 1.0m,
            CarbsG = 50,
            FatG = 0.6m
        };

        var foodEntry2 = new FoodEntry
        {
            FoodEntryId = 2,
            FoodItemId = 2,
            FoodItem = foodItem2,
            Quantity = 1,
            MealType = "Lunch",
            CaloriesKcal = 105,
            ProteinG = 1.3m,
            CarbsG = 27,
            FatG = 0.4m
        };

        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Today,
            TotalCalories = 295,
            ProteinG = 2.3m,
            CarbsG = 77,
            FatG = 1.0m,
            FoodEntries = new List<FoodEntry> { foodEntry1, foodEntry2 }
        };

        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery { UserId = 1, Date = DateTime.Today };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.FoodEntries.Count);
        Assert.Equal("Apple", result.FoodEntries[0].FoodItemName);
        Assert.Equal("Banana", result.FoodEntries[1].FoodItemName);
        Assert.Equal(2, result.FoodEntries[0].Quantity);
        Assert.Equal(190, result.FoodEntries[0].CaloriesKcal);
    }

    [Fact]
    public async Task Handle_ShouldHandleNullNutrientValues()
    {
        // Arrange
        var foodItem = new FoodItem { FoodItemId = 1, Name = "Water" };
        var foodEntry = new FoodEntry
        {
            FoodEntryId = 1,
            FoodItemId = 1,
            FoodItem = foodItem,
            Quantity = 1,
            MealType = "Snack",
            CaloriesKcal = null,
            ProteinG = null,
            CarbsG = null,
            FatG = null
        };

        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = DateTime.Today,
            TotalCalories = 0,
            ProteinG = 0,
            CarbsG = 0,
            FatG = 0,
            FoodEntries = new List<FoodEntry> { foodEntry }
        };

        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery { UserId = 1, Date = DateTime.Today };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.FoodEntries[0].CaloriesKcal);
        Assert.Equal(0, result.FoodEntries[0].ProteinG);
        Assert.Equal(0, result.FoodEntries[0].CarbsG);
        Assert.Equal(0, result.FoodEntries[0].FatG);
    }

    [Fact]
    public async Task Handle_ShouldMatchDateIgnoringTime()
    {
        // Arrange
        var nutritionLog = new NutritionLog
        {
            NutritionLogId = 1,
            UserId = 1,
            LogDate = new DateTime(2024, 1, 15, 14, 30, 0), // With time component
            TotalCalories = 100,
            FoodEntries = new List<FoodEntry>()
        };

        var nutritionLogs = new List<NutritionLog> { nutritionLog };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        var query = new GetNutritionLogByDateQuery 
        { 
            UserId = 1, 
            Date = new DateTime(2024, 1, 15, 8, 0, 0) // Different time, same date
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.NutritionLogId);
    }
}
