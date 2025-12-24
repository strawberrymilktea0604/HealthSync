using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;

namespace HealthSync.Application.Tests.Handlers;

public class CreateNutritionLogCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateNutritionLogCommandHandler _handler;

    public CreateNutritionLogCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateNutritionLogCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesNutritionLogSuccessfully()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Chicken Breast",
                ServingSize = 100,
                CaloriesKcal = 165,
                ProteinG = 31,
                CarbsG = 0,
                FatG = 3.6m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

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
                        Quantity = 150, // 1.5 servings
                        MealType = "Lunch"
                    }
                }
            }
        };

        NutritionLog? capturedLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<NutritionLog>()))
            .Callback<NutritionLog>(log => capturedLog = log);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (capturedLog != null)
                {
                    capturedLog.NutritionLogId = 100;
                }
            })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(100, result);
        Assert.NotNull(capturedLog);
        Assert.Equal(1, capturedLog.UserId);
        Assert.Single(capturedLog.FoodEntries);
    }

    [Fact]
    public async Task Handle_CalculatesTotalCalories_Correctly()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Rice",
                ServingSize = 100,
                CaloriesKcal = 130,
                ProteinG = 2.7m,
                CarbsG = 28,
                FatG = 0.3m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

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
                        Quantity = 200, // 2 servings = 260 calories
                        MealType = "Dinner"
                    }
                }
            }
        };

        NutritionLog? capturedLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<NutritionLog>()))
            .Callback<NutritionLog>(log => capturedLog = log);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(260, capturedLog.TotalCalories);
    }

    [Fact]
    public async Task Handle_MultipleFoodEntries_CalculatesTotalCorrectly()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Egg",
                ServingSize = 1,
                CaloriesKcal = 70,
                ProteinG = 6,
                CarbsG = 0.6m,
                FatG = 5
            },
            new FoodItem
            {
                FoodItemId = 2,
                Name = "Toast",
                ServingSize = 1,
                CaloriesKcal = 80,
                ProteinG = 3,
                CarbsG = 15,
                FatG = 1
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

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
                        Quantity = 2, // 140 calories
                        MealType = "Breakfast"
                    },
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 2,
                        Quantity = 2, // 160 calories
                        MealType = "Breakfast"
                    }
                }
            }
        };

        NutritionLog? capturedLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<NutritionLog>()))
            .Callback<NutritionLog>(log => capturedLog = log);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(2, capturedLog.FoodEntries.Count);
        Assert.Equal(300, capturedLog.TotalCalories); // 140 + 160
    }

    [Fact]
    public async Task Handle_CalculatesMacronutrients_Correctly()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Salmon",
                ServingSize = 100,
                CaloriesKcal = 206,
                ProteinG = 22,
                CarbsG = 0,
                FatG = 13
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

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
                        Quantity = 150, // 1.5 servings
                        MealType = "Dinner"
                    }
                }
            }
        };

        NutritionLog? capturedLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<NutritionLog>()))
            .Callback<NutritionLog>(log => capturedLog = log);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        var entry = capturedLog.FoodEntries.First();
        Assert.Equal(309, entry.CaloriesKcal); // 206 * 1.5
        Assert.Equal(33, entry.ProteinG); // 22 * 1.5
        Assert.Equal(0, entry.CarbsG); // 0 * 1.5
        Assert.Equal(19.5m, entry.FatG); // 13 * 1.5
    }

    [Fact]
    public async Task Handle_FoodItemNotFound_CreatesEntryWithoutNutritionData()
    {
        // Arrange
        var foodItems = new List<FoodItem>(); // Empty list

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

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
                        FoodItemId = 999, // Non-existent
                        Quantity = 100,
                        MealType = "Snack"
                    }
                }
            }
        };

        NutritionLog? capturedLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<NutritionLog>()))
            .Callback<NutritionLog>(log => capturedLog = log);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(0, capturedLog.TotalCalories);
        var entry = capturedLog.FoodEntries.First();
        Assert.Null(entry.CaloriesKcal);
    }

    [Fact]
    public async Task Handle_PreservesLogDate_Correctly()
    {
        // Arrange
        var specificDate = new DateTime(2024, 12, 20);
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Apple",
                ServingSize = 1,
                CaloriesKcal = 52,
                ProteinG = 0.3m,
                CarbsG = 14,
                FatG = 0.2m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

        var command = new CreateNutritionLogCommand
        {
            UserId = 1,
            NutritionLog = new CreateNutritionLogDto
            {
                LogDate = specificDate,
                FoodEntries = new List<CreateFoodEntryDto>
                {
                    new CreateFoodEntryDto
                    {
                        FoodItemId = 1,
                        Quantity = 1,
                        MealType = "Snack"
                    }
                }
            }
        };

        NutritionLog? capturedLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<NutritionLog>()))
            .Callback<NutritionLog>(log => capturedLog = log);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(specificDate, capturedLog.LogDate);
    }

    [Fact]
    public async Task Handle_CallsContextAddOnce()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Banana",
                ServingSize = 1,
                CaloriesKcal = 89,
                ProteinG = 1.1m,
                CarbsG = 23,
                FatG = 0.3m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.FoodItems).Returns(mockFoodItems);

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
                        Quantity = 1,
                        MealType = "Breakfast"
                    }
                }
            }
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.Add(It.IsAny<NutritionLog>()), Times.Once);
    }
}
