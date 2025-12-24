using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetNutritionLogsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetNutritionLogsQueryHandler _handler;

    public GetNutritionLogsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetNutritionLogsQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllLogsForUser_WhenNoDateFilters()
    {
        // Arrange
        var userId = 1;
        var nutritionLogs = new List<NutritionLog>
        {
            new NutritionLog
            {
                NutritionLogId = 1,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 15),
                TotalCalories = 1500,
                FoodEntries = new List<FoodEntry>
                {
                    new FoodEntry
                    {
                        FoodEntryId = 1,
                        FoodItemId = 1,
                        Quantity = 100,
                        MealType = "Breakfast",
                        CaloriesKcal = 250,
                        ProteinG = 10,
                        CarbsG = 30,
                        FatG = 8,
                        FoodItem = new FoodItem { FoodItemId = 1, Name = "Oatmeal" }
                    }
                }
            },
            new NutritionLog
            {
                NutritionLogId = 2,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 16),
                TotalCalories = 1800,
                FoodEntries = new List<FoodEntry>()
            }
        };

        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1500, result[0].TotalCalories);
        Assert.Equal(1800, result[1].TotalCalories);
        Assert.Single(result[0].FoodEntries);
        Assert.Equal("Oatmeal", result[0].FoodEntries[0].FoodItemName);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStartDate_WhenStartDateProvided()
    {
        // Arrange
        var userId = 1;
        var nutritionLogs = new List<NutritionLog>
        {
            new NutritionLog
            {
                NutritionLogId = 1,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 10),
                TotalCalories = 1400,
                FoodEntries = new List<FoodEntry>()
            },
            new NutritionLog
            {
                NutritionLogId = 2,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 20),
                TotalCalories = 1600,
                FoodEntries = new List<FoodEntry>()
            }
        };

        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery
        {
            UserId = userId,
            StartDate = new DateTime(2024, 1, 15)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].NutritionLogId);
        Assert.Equal(new DateTime(2024, 1, 20), result[0].LogDate);
    }

    [Fact]
    public async Task Handle_ShouldFilterByEndDate_WhenEndDateProvided()
    {
        // Arrange
        var userId = 1;
        var nutritionLogs = new List<NutritionLog>
        {
            new NutritionLog
            {
                NutritionLogId = 1,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 10),
                TotalCalories = 1400,
                FoodEntries = new List<FoodEntry>()
            },
            new NutritionLog
            {
                NutritionLogId = 2,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 20),
                TotalCalories = 1600,
                FoodEntries = new List<FoodEntry>()
            }
        };

        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery
        {
            UserId = userId,
            EndDate = new DateTime(2024, 1, 15)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].NutritionLogId);
        Assert.Equal(new DateTime(2024, 1, 10), result[0].LogDate);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateRange_WhenBothDatesProvided()
    {
        // Arrange
        var userId = 1;
        var nutritionLogs = new List<NutritionLog>
        {
            new NutritionLog
            {
                NutritionLogId = 1,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 5),
                TotalCalories = 1300,
                FoodEntries = new List<FoodEntry>()
            },
            new NutritionLog
            {
                NutritionLogId = 2,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 15),
                TotalCalories = 1500,
                FoodEntries = new List<FoodEntry>()
            },
            new NutritionLog
            {
                NutritionLogId = 3,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 25),
                TotalCalories = 1700,
                FoodEntries = new List<FoodEntry>()
            }
        };

        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery
        {
            UserId = userId,
            StartDate = new DateTime(2024, 1, 10),
            EndDate = new DateTime(2024, 1, 20)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].NutritionLogId);
        Assert.Equal(new DateTime(2024, 1, 15), result[0].LogDate);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyUserLogs_WhenMultipleUsersExist()
    {
        // Arrange
        var userId = 1;
        var nutritionLogs = new List<NutritionLog>
        {
            new NutritionLog
            {
                NutritionLogId = 1,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 15),
                TotalCalories = 1500,
                FoodEntries = new List<FoodEntry>()
            },
            new NutritionLog
            {
                NutritionLogId = 2,
                UserId = 2, // Different user
                LogDate = new DateTime(2024, 1, 15),
                TotalCalories = 1600,
                FoodEntries = new List<FoodEntry>()
            }
        };

        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(userId, result[0].UserId);
        Assert.Equal(1, result[0].NutritionLogId);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoLogsFound()
    {
        // Arrange
        var nutritionLogs = new List<NutritionLog>();
        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery { UserId = 999 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldIncludeFoodEntriesWithNutrients_WhenAvailable()
    {
        // Arrange
        var userId = 1;
        var nutritionLogs = new List<NutritionLog>
        {
            new NutritionLog
            {
                NutritionLogId = 1,
                UserId = userId,
                LogDate = new DateTime(2024, 1, 15),
                TotalCalories = 500,
                FoodEntries = new List<FoodEntry>
                {
                    new FoodEntry
                    {
                        FoodEntryId = 1,
                        FoodItemId = 1,
                        Quantity = 200,
                        MealType = "Lunch",
                        CaloriesKcal = 300,
                        ProteinG = 15,
                        CarbsG = 40,
                        FatG = 10,
                        FoodItem = new FoodItem { FoodItemId = 1, Name = "Chicken Salad" }
                    },
                    new FoodEntry
                    {
                        FoodEntryId = 2,
                        FoodItemId = 2,
                        Quantity = 150,
                        MealType = "Snack",
                        CaloriesKcal = 200,
                        ProteinG = 5,
                        CarbsG = 30,
                        FatG = 8,
                        FoodItem = new FoodItem { FoodItemId = 2, Name = "Protein Bar" }
                    }
                }
            }
        };

        var mockNutritionLogs = nutritionLogs.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.NutritionLogs).Returns(mockNutritionLogs);

        var query = new GetNutritionLogsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].FoodEntries.Count);
        
        var firstEntry = result[0].FoodEntries[0];
        Assert.Equal("Chicken Salad", firstEntry.FoodItemName);
        Assert.Equal(300, firstEntry.CaloriesKcal);
        Assert.Equal(15, firstEntry.ProteinG);
        Assert.Equal(40, firstEntry.CarbsG);
        Assert.Equal(10, firstEntry.FatG);

        var secondEntry = result[0].FoodEntries[1];
        Assert.Equal("Protein Bar", secondEntry.FoodItemName);
        Assert.Equal(200, secondEntry.CaloriesKcal);
    }
}
