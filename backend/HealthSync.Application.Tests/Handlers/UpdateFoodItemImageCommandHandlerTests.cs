using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateFoodItemImageCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateFoodItemImageCommandHandler _handler;

    public UpdateFoodItemImageCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateFoodItemImageCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateInternalImage_WhenFoodItemExists()
    {
        // Arrange
        var command = new UpdateFoodItemImageCommand
        {
            FoodItemId = 1,
            ImageUrl = "http://example.com/new-food.jpg"
        };

        var foodItem = new FoodItem
        {
            FoodItemId = 1,
            ImageUrl = "http://example.com/old-food.jpg"
        };

        var foodItemsList = new List<FoodItem> { foodItem };
        var foodItemsMock = foodItemsList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.FoodItems).Returns(foodItemsMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        Assert.Equal("http://example.com/new-food.jpg", foodItem.ImageUrl);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenFoodItemNotFound()
    {
        // Arrange
        var command = new UpdateFoodItemImageCommand
        {
            FoodItemId = 999,
            ImageUrl = "http://example.com/any.jpg"
        };

        var foodItemsList = new List<FoodItem>();
        var foodItemsMock = foodItemsList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.FoodItems).Returns(foodItemsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Không tìm thấy món ăn", exception.Message);
        
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
