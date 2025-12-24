using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetFoodItemByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetFoodItemByIdQueryHandler _handler;

    public GetFoodItemByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetFoodItemByIdQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFoodItem_WhenFoodItemExists()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Chicken Breast",
                ServingSize = 100,
                ServingUnit = "g",
                CaloriesKcal = 165,
                ProteinG = 31,
                CarbsG = 0,
                FatG = 3.6m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemByIdQuery { FoodItemId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.FoodItemId);
        Assert.Equal("Chicken Breast", result.Name);
        Assert.Equal(100, result.ServingSize);
        Assert.Equal("g", result.ServingUnit);
        Assert.Equal(165, result.CaloriesKcal);
        Assert.Equal(31, result.ProteinG);
        Assert.Equal(0, result.CarbsG);
        Assert.Equal(3.6m, result.FatG);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenFoodItemDoesNotExist()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem { FoodItemId = 1, Name = "Rice", ServingSize = 100, ServingUnit = "g" }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemByIdQuery { FoodItemId = 999 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnFoodItemWithDecimalValues_WhenNutrientsAreDecimals()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 2,
                Name = "Salmon",
                ServingSize = 100,
                ServingUnit = "g",
                CaloriesKcal = 208,
                ProteinG = 20.4m,
                CarbsG = 0,
                FatG = 13.4m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemByIdQuery { FoodItemId = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20.4m, result!.ProteinG);
        Assert.Equal(13.4m, result.FatG);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFoodItem_WhenMultipleFoodItemsExist()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem { FoodItemId = 1, Name = "Apple", ServingSize = 1, ServingUnit = "medium" },
            new FoodItem { FoodItemId = 2, Name = "Banana", ServingSize = 1, ServingUnit = "medium" },
            new FoodItem { FoodItemId = 3, Name = "Orange", ServingSize = 1, ServingUnit = "medium" }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemByIdQuery { FoodItemId = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.FoodItemId);
        Assert.Equal("Banana", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnFoodItemWithZeroCalories_WhenCaloriesAreZero()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 5,
                Name = "Water",
                ServingSize = 250,
                ServingUnit = "ml",
                CaloriesKcal = 0,
                ProteinG = 0,
                CarbsG = 0,
                FatG = 0
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemByIdQuery { FoodItemId = 5 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Water", result!.Name);
        Assert.Equal(0, result.CaloriesKcal);
        Assert.Equal(0, result.ProteinG);
    }

    [Fact]
    public async Task Handle_ShouldReturnFoodItemWithDifferentServingUnits_WhenUnitsVary()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 10,
                Name = "Milk",
                ServingSize = 1,
                ServingUnit = "cup",
                CaloriesKcal = 149,
                ProteinG = 7.7m,
                CarbsG = 11.7m,
                FatG = 7.9m
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemByIdQuery { FoodItemId = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.ServingSize);
        Assert.Equal("cup", result.ServingUnit);
    }
}
