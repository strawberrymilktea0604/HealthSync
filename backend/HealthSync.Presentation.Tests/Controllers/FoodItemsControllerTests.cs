using System.Security.Claims;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HealthSync.Presentation.Tests.Controllers;

public class FoodItemsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IStorageService> _storageServiceMock;

    private readonly FoodItemsController _controller;

    public FoodItemsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _storageServiceMock = new Mock<IStorageService>();
        _controller = new FoodItemsController(_mediatorMock.Object, _storageServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetFoodItems_WithoutSearch_ReturnsAllFoodItems()
    {
        // Arrange
        var expectedFoodItems = new List<FoodItemDto>
        {
            new FoodItemDto
            {
                FoodItemId = 1,
                Name = "Chicken Breast",
                ServingSize = 100,
                ServingUnit = "g",
                CaloriesKcal = 165,
                ProteinG = 31,
                CarbsG = 0,
                FatG = 3.6m
            },
            new FoodItemDto
            {
                FoodItemId = 2,
                Name = "Brown Rice",
                ServingSize = 100,
                ServingUnit = "g",
                CaloriesKcal = 112,
                ProteinG = 2.3m,
                CarbsG = 24,
                FatG = 0.9m
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFoodItemsQuery>(), default))
            .ReturnsAsync(expectedFoodItems);

        // Act
        var result = await _controller.GetFoodItems(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var foodItems = Assert.IsAssignableFrom<List<FoodItemDto>>(okResult.Value);
        Assert.Equal(2, foodItems.Count);
    }

    [Fact]
    public async Task GetFoodItems_WithSearchTerm_ReturnsFilteredFoodItems()
    {
        // Arrange
        var searchTerm = "chicken";
        var expectedFoodItems = new List<FoodItemDto>
        {
            new FoodItemDto
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

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetFoodItemsQuery>(q => q.Search == searchTerm), default))
            .ReturnsAsync(expectedFoodItems);

        // Act
        var result = await _controller.GetFoodItems(searchTerm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var foodItems = Assert.IsAssignableFrom<List<FoodItemDto>>(okResult.Value);
        Assert.Single(foodItems);
        Assert.Contains("Chicken", foodItems[0].Name);
    }

    [Fact]
    public async Task GetFoodItemById_WithValidId_ReturnsOkWithFoodItem()
    {
        // Arrange
        var foodItemId = 1;
        var expectedFoodItem = new FoodItemDto
        {
            FoodItemId = foodItemId,
            Name = "Chicken Breast",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 165,
            ProteinG = 31,
            CarbsG = 0,
            FatG = 3.6m
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetFoodItemByIdQuery>(q => q.FoodItemId == foodItemId), default))
            .ReturnsAsync(expectedFoodItem);

        // Act
        var result = await _controller.GetFoodItemById(foodItemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var foodItem = Assert.IsType<FoodItemDto>(okResult.Value);
        Assert.Equal(expectedFoodItem.FoodItemId, foodItem.FoodItemId);
        Assert.Equal(expectedFoodItem.Name, foodItem.Name);
    }

    [Fact]
    public async Task GetFoodItemById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var foodItemId = 999;

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetFoodItemByIdQuery>(q => q.FoodItemId == foodItemId), default))
            .ReturnsAsync((FoodItemDto?)null);

        // Act
        var result = await _controller.GetFoodItemById(foodItemId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateFoodItem_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var command = new CreateFoodItemCommand
        {
            Name = "New Food Item",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 200,
            ProteinG = 20,
            CarbsG = 15,
            FatG = 5
        };

        var expectedId = 10;

        _mediatorMock
            .Setup(m => m.Send(command, default))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _controller.CreateFoodItem(command);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FoodItemsController.GetFoodItemById), createdResult.ActionName);
        var routeValues = (Microsoft.AspNetCore.Routing.RouteValueDictionary)createdResult.RouteValues!;
        var actualId = (int)routeValues["id"]!;
        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public async Task CreateFoodItem_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateFoodItemCommand();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateFoodItem(command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFoodItem_WithValidData_ReturnsOk()
    {
        // Arrange
        var foodItemId = 1;
        var command = new UpdateFoodItemCommand
        {
            FoodItemId = foodItemId,
            Name = "Updated Food Item",
            ServingSize = 150,
            ServingUnit = "g",
            CaloriesKcal = 250,
            ProteinG = 25,
            CarbsG = 20,
            FatG = 8
        };

        _mediatorMock
            .Setup(m => m.Send(command, default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateFoodItem(foodItemId, command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdateFoodItem_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var urlId = 1;
        var command = new UpdateFoodItemCommand
        {
            FoodItemId = 2,
            Name = "Updated Food Item",
            ServingSize = 150,
            ServingUnit = "g",
            CaloriesKcal = 250,
            ProteinG = 25,
            CarbsG = 20,
            FatG = 8
        };

        // Act
        var result = await _controller.UpdateFoodItem(urlId, command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFoodItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var foodItemId = 999;
        var command = new UpdateFoodItemCommand
        {
            FoodItemId = foodItemId,
            Name = "Updated Food Item",
            ServingSize = 150,
            ServingUnit = "g",
            CaloriesKcal = 250,
            ProteinG = 25,
            CarbsG = 20,
            FatG = 8
        };

        _mediatorMock
            .Setup(m => m.Send(command, default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateFoodItem(foodItemId, command);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFoodItem_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var foodItemId = 1;
        var command = new UpdateFoodItemCommand { FoodItemId = foodItemId };
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateFoodItem(foodItemId, command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteFoodItem_WithValidId_ReturnsOk()
    {
        // Arrange
        var foodItemId = 1;

        _mediatorMock
            .Setup(m => m.Send(It.Is<DeleteFoodItemCommand>(c => c.FoodItemId == foodItemId), default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFoodItem(foodItemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteFoodItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var foodItemId = 999;

        _mediatorMock
            .Setup(m => m.Send(It.Is<DeleteFoodItemCommand>(c => c.FoodItemId == foodItemId), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFoodItem(foodItemId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteFoodItem_WhenInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var foodItemId = 1;

        _mediatorMock
            .Setup(m => m.Send(It.Is<DeleteFoodItemCommand>(c => c.FoodItemId == foodItemId), default))
            .ThrowsAsync(new InvalidOperationException("Cannot delete food item in use"));

        // Act
        var result = await _controller.DeleteFoodItem(foodItemId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}
