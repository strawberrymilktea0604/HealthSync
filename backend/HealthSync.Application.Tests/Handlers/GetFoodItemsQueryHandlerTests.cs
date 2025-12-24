using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetFoodItemsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetFoodItemsQueryHandler _handler;

    public GetFoodItemsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetFoodItemsQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllFoodItems_WhenNoSearchProvided()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem { FoodItemId = 1, Name = "Chicken Breast", CaloriesKcal = 165m, ProteinG = 31m, CarbsG = 0m, FatG = 3.6m },
            new FoodItem { FoodItemId = 2, Name = "Brown Rice", CaloriesKcal = 110m, ProteinG = 2.6m, CarbsG = 23m, FatG = 0.9m },
            new FoodItem { FoodItemId = 3, Name = "Broccoli", CaloriesKcal = 34m, ProteinG = 2.8m, CarbsG = 7m, FatG = 0.4m }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldFilterByName_WhenSearchTermProvided()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem { FoodItemId = 1, Name = "Chicken Breast", CaloriesKcal = 165m },
            new FoodItem { FoodItemId = 2, Name = "Chicken Thigh", CaloriesKcal = 209m },
            new FoodItem { FoodItemId = 3, Name = "Brown Rice", CaloriesKcal = 110m }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemsQuery { Search = "Chicken" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Contains("Chicken", f.Name));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFoodItemsMatchSearch()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem { FoodItemId = 1, Name = "Chicken Breast", CaloriesKcal = 165m }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemsQuery { Search = "Beef" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFoodItemsExist()
    {
        // Arrange
        var foodItems = new List<FoodItem>();
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldIncludeAllNutrientFields_WhenReturningFoodItems()
    {
        // Arrange
        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                FoodItemId = 1,
                Name = "Salmon",
                CaloriesKcal = 206m,
                ProteinG = 22m,
                CarbsG = 0m,
                FatG = 13m,
                ServingSize = 100m,
                ServingUnit = "g"
            }
        };

        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var query = new GetFoodItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var foodItem = result[0];
        Assert.Equal(206m, foodItem.CaloriesKcal);
        Assert.Equal(22m, foodItem.ProteinG);
        Assert.Equal(0m, foodItem.CarbsG);
        Assert.Equal(13m, foodItem.FatG);
        Assert.Equal(100m, foodItem.ServingSize);
        Assert.Equal("g", foodItem.ServingUnit);
    }
}
