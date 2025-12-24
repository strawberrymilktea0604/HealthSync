using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateFoodItemCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateFoodItemCommandHandler _handler;

    public UpdateFoodItemCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateFoodItemCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateFoodItem_WhenFoodItemExists()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Old Name",
            ServingSize = 50,
            ServingUnit = "g",
            CaloriesKcal = 100,
            ProteinG = 5,
            CarbsG = 10,
            FatG = 2
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 1,
            Name = "Updated Chicken",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 165,
            ProteinG = 31,
            CarbsG = 0,
            FatG = 3.6m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Updated Chicken", foodItem.Name);
        Assert.Equal(100, foodItem.ServingSize);
        Assert.Equal("g", foodItem.ServingUnit);
        Assert.Equal(165, foodItem.CaloriesKcal);
        Assert.Equal(31, foodItem.ProteinG);
        Assert.Equal(0, foodItem.CarbsG);
        Assert.Equal(3.6m, foodItem.FatG);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenFoodItemDoesNotExist()
    {
        // Arrange
        var foodItems = new List<FoodItem>();
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 999,
            Name = "Non-existent Item",
            ServingSize = 100,
            ServingUnit = "g"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateDecimalValues_WhenNutrientsAreDecimals()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 2,
            Name = "Salmon",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 200,
            ProteinG = 20,
            CarbsG = 0,
            FatG = 10
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 2,
            Name = "Wild Salmon",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 208,
            ProteinG = 20.4m,
            CarbsG = 0,
            FatG = 13.4m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Wild Salmon", foodItem.Name);
        Assert.Equal(20.4m, foodItem.ProteinG);
        Assert.Equal(13.4m, foodItem.FatG);
    }

    [Fact]
    public async Task Handle_ShouldUpdateServingUnit_WhenUnitChanges()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 3,
            Name = "Milk",
            ServingSize = 250,
            ServingUnit = "ml",
            CaloriesKcal = 150,
            ProteinG = 8,
            CarbsG = 12,
            FatG = 8
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 3,
            Name = "Whole Milk",
            ServingSize = 1,
            ServingUnit = "cup",
            CaloriesKcal = 149,
            ProteinG = 7.7m,
            CarbsG = 11.7m,
            FatG = 7.9m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Whole Milk", foodItem.Name);
        Assert.Equal(1, foodItem.ServingSize);
        Assert.Equal("cup", foodItem.ServingUnit);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyTargetFoodItem_WhenMultipleFoodItemsExist()
    {
        // Arrange
        var targetFoodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Apple",
            ServingSize = 1,
            ServingUnit = "medium",
            CaloriesKcal = 95
        };

        var otherFoodItem = new FoodItem
        {
            FoodItemId = 2,
            Name = "Banana",
            ServingSize = 1,
            ServingUnit = "medium",
            CaloriesKcal = 105
        };

        var foodItems = new List<FoodItem> { targetFoodItem, otherFoodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 1,
            Name = "Red Apple",
            ServingSize = 1,
            ServingUnit = "large",
            CaloriesKcal = 116,
            ProteinG = 0.6m,
            CarbsG = 31,
            FatG = 0.4m
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Red Apple", targetFoodItem.Name);
        Assert.Equal("large", targetFoodItem.ServingUnit);
        // Other food item should remain unchanged
        Assert.Equal("Banana", otherFoodItem.Name);
        Assert.Equal(105, otherFoodItem.CaloriesKcal);
    }

    [Fact]
    public async Task Handle_ShouldUpdateToZeroValues_WhenNutrientsAreZero()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 5,
            Name = "Sugary Water",
            ServingSize = 250,
            ServingUnit = "ml",
            CaloriesKcal = 100,
            ProteinG = 0,
            CarbsG = 25,
            FatG = 0
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 5,
            Name = "Water",
            ServingSize = 250,
            ServingUnit = "ml",
            CaloriesKcal = 0,
            ProteinG = 0,
            CarbsG = 0,
            FatG = 0
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Water", foodItem.Name);
        Assert.Equal(0, foodItem.CaloriesKcal);
        Assert.Equal(0, foodItem.CarbsG);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_OnlyWhenFoodItemFound()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 10,
            Name = "Rice",
            ServingSize = 100,
            ServingUnit = "g"
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 10,
            Name = "Brown Rice",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 112,
            ProteinG = 2.6m,
            CarbsG = 23.5m,
            FatG = 0.9m
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
