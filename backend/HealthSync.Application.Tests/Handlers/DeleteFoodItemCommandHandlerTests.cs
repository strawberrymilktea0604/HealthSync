using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class DeleteFoodItemCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteFoodItemCommandHandler _handler;

    public DeleteFoodItemCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteFoodItemCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFoodItem_WhenFoodItemExistsAndNotUsed()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Chicken Breast",
            ServingSize = 100,
            ServingUnit = "g"
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var foodEntries = new List<FoodEntry>();
        var mockEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockEntries);

        var command = new DeleteFoodItemCommand { FoodItemId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Remove(foodItem), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenFoodItemDoesNotExist()
    {
        // Arrange
        var foodItems = new List<FoodItem>();
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var command = new DeleteFoodItemCommand { FoodItemId = 999 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<FoodItem>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenFoodItemIsBeingUsed()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            Name = "Brown Rice",
            ServingSize = 100,
            ServingUnit = "g"
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var foodEntries = new List<FoodEntry>
        {
            new FoodEntry { FoodItemId = 1, NutritionLogId = 1 }
        };
        var mockEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockEntries);

        var command = new DeleteFoodItemCommand { FoodItemId = 1 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Không thể xóa món ăn đang được sử dụng", exception.Message);
        _contextMock.Verify(c => c.Remove(It.IsAny<FoodItem>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCheckFoodItemUsage_BeforeDeleting()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 2,
            Name = "Salmon",
            ServingSize = 100,
            ServingUnit = "g"
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var foodEntries = new List<FoodEntry>();
        var mockEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockEntries);

        var command = new DeleteFoodItemCommand { FoodItemId = 2 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify that usage check was performed
        _contextMock.Verify(c => c.FoodEntries, Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteOtherFoodItems_WhenDeletingOne()
    {
        // Arrange
        var foodItemToDelete = new FoodItem
        {
            FoodItemId = 1,
            Name = "Apple",
            ServingSize = 1,
            ServingUnit = "medium"
        };

        var otherFoodItem = new FoodItem
        {
            FoodItemId = 2,
            Name = "Banana",
            ServingSize = 1,
            ServingUnit = "medium"
        };

        var foodItems = new List<FoodItem> { foodItemToDelete, otherFoodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        var foodEntries = new List<FoodEntry>();
        var mockEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockEntries);

        var command = new DeleteFoodItemCommand { FoodItemId = 1 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Only the target food item should be removed
        _contextMock.Verify(c => c.Remove(foodItemToDelete), Times.Once);
        _contextMock.Verify(c => c.Remove(otherFoodItem), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFoodItemWithMultipleUsageChecks_WhenNotUsedInAnyEntry()
    {
        // Arrange
        var foodItem = new FoodItem
        {
            FoodItemId = 5,
            Name = "Water",
            ServingSize = 250,
            ServingUnit = "ml"
        };

        var foodItems = new List<FoodItem> { foodItem };
        var mockFoodItems = foodItems.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodItems).Returns(mockFoodItems);

        // Other food items are used but not this one
        var foodEntries = new List<FoodEntry>
        {
            new FoodEntry { FoodItemId = 10, NutritionLogId = 1 },
            new FoodEntry { FoodItemId = 20, NutritionLogId = 2 }
        };
        var mockEntries = foodEntries.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.FoodEntries).Returns(mockEntries);

        var command = new DeleteFoodItemCommand { FoodItemId = 5 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Remove(foodItem), Times.Once);
    }
}
