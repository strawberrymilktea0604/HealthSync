using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class CreateFoodItemCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateFoodItemCommandHandler _handler;

    public CreateFoodItemCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateFoodItemCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateFoodItem_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Chicken Breast",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 165,
            ProteinG = 31,
            CarbsG = 0,
            FatG = 3.6m
        };

        FoodItem? capturedFoodItem = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodItem>()))
            .Callback<object>(e => capturedFoodItem = e as FoodItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.Add(It.IsAny<FoodItem>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.NotNull(capturedFoodItem);
        Assert.Equal("Chicken Breast", capturedFoodItem!.Name);
        Assert.Equal(100, capturedFoodItem.ServingSize);
        Assert.Equal("g", capturedFoodItem.ServingUnit);
        Assert.Equal(165, capturedFoodItem.CaloriesKcal);
        Assert.Equal(31, capturedFoodItem.ProteinG);
        Assert.Equal(0, capturedFoodItem.CarbsG);
        Assert.Equal(3.6m, capturedFoodItem.FatG);
    }

    [Fact]
    public async Task Handle_ShouldReturnFoodItemId_AfterCreation()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Brown Rice",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 112,
            ProteinG = 2.6m,
            CarbsG = 23.5m,
            FatG = 0.9m
        };

        _contextMock.Setup(c => c.Add(It.IsAny<FoodItem>()))
            .Callback<object>(e =>
            {
                if (e is FoodItem foodItem)
                {
                    foodItem.FoodItemId = 10;
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public async Task Handle_ShouldCreateFoodItemWithDecimalValues_WhenNutrientsAreDecimals()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Salmon",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 208,
            ProteinG = 20.4m,
            CarbsG = 0,
            FatG = 13.4m
        };

        FoodItem? capturedFoodItem = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodItem>()))
            .Callback<object>(e => capturedFoodItem = e as FoodItem);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedFoodItem);
        Assert.Equal(20.4m, capturedFoodItem!.ProteinG);
        Assert.Equal(13.4m, capturedFoodItem.FatG);
    }

    [Fact]
    public async Task Handle_ShouldCreateFoodItemWithZeroValues_WhenNutrientsAreZero()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Water",
            ServingSize = 250,
            ServingUnit = "ml",
            CaloriesKcal = 0,
            ProteinG = 0,
            CarbsG = 0,
            FatG = 0
        };

        FoodItem? capturedFoodItem = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodItem>()))
            .Callback<object>(e => capturedFoodItem = e as FoodItem);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedFoodItem);
        Assert.Equal("Water", capturedFoodItem!.Name);
        Assert.Equal(0, capturedFoodItem.CaloriesKcal);
        Assert.Equal(0, capturedFoodItem.ProteinG);
        Assert.Equal(0, capturedFoodItem.CarbsG);
        Assert.Equal(0, capturedFoodItem.FatG);
    }

    [Fact]
    public async Task Handle_ShouldCreateFoodItemWithDifferentServingUnits_WhenUnitsVary()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Milk",
            ServingSize = 1,
            ServingUnit = "cup",
            CaloriesKcal = 149,
            ProteinG = 7.7m,
            CarbsG = 11.7m,
            FatG = 7.9m
        };

        FoodItem? capturedFoodItem = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodItem>()))
            .Callback<object>(e => capturedFoodItem = e as FoodItem);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedFoodItem);
        Assert.Equal(1, capturedFoodItem!.ServingSize);
        Assert.Equal("cup", capturedFoodItem.ServingUnit);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Banana",
            ServingSize = 1,
            ServingUnit = "medium",
            CaloriesKcal = 105,
            ProteinG = 1.3m,
            CarbsG = 27,
            FatG = 0.4m
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateFoodItemWithHighCalories_WhenFoodIsCalorieDense()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "Peanut Butter",
            ServingSize = 2,
            ServingUnit = "tablespoons",
            CaloriesKcal = 188,
            ProteinG = 8,
            CarbsG = 7,
            FatG = 16
        };

        FoodItem? capturedFoodItem = null;
        _contextMock.Setup(c => c.Add(It.IsAny<FoodItem>()))
            .Callback<object>(e => capturedFoodItem = e as FoodItem);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedFoodItem);
        Assert.Equal("Peanut Butter", capturedFoodItem!.Name);
        Assert.Equal(188, capturedFoodItem.CaloriesKcal);
        Assert.Equal(16, capturedFoodItem.FatG);
    }
}
