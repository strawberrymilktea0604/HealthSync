using System.Security.Claims;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class NutritionControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NutritionController _controller;

    public NutritionControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new NutritionController(_mediatorMock.Object);
    }

    private void SetupUserClaims(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("uid", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetFoodItems_WithoutSearch_ReturnsAllFoodItems()
    {
        // Arrange
        var foodItems = new List<FoodItemDto>
        {
            new FoodItemDto { FoodItemId = 1, Name = "Chicken Breast", CaloriesKcal = 165 },
            new FoodItemDto { FoodItemId = 2, Name = "Brown Rice", CaloriesKcal = 112 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetFoodItemsQuery>(), default))
            .ReturnsAsync(foodItems);

        // Act
        var result = await _controller.GetFoodItems(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<FoodItemDto>>(okResult.Value);
        Assert.Equal(2, returnedItems.Count());
    }

    [Fact]
    public async Task GetFoodItems_WithSearchTerm_ReturnsFilteredItems()
    {
        // Arrange
        var search = "Chicken";
        var foodItems = new List<FoodItemDto>
        {
            new FoodItemDto { FoodItemId = 1, Name = "Chicken Breast", CaloriesKcal = 165 }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetFoodItemsQuery>(q => q.Search == search),
            default))
            .ReturnsAsync(foodItems);

        // Act
        var result = await _controller.GetFoodItems(search);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<FoodItemDto>>(okResult.Value);
        Assert.Single(returnedItems);
        Assert.Contains("Chicken", returnedItems.First().Name);
    }

    [Fact]
    public async Task GetNutritionLogByDate_WithValidUser_ReturnsLog()
    {
        // Arrange
        var userId = 1;
        var date = DateTime.UtcNow.Date;
        SetupUserClaims(userId);

        var nutritionLog = new NutritionLogDto
        {
            NutritionLogId = 1,
            UserId = userId,
            LogDate = date,
            TotalCalories = 2000
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetNutritionLogByDateQuery>(q => q.UserId == userId && q.Date == date),
            default))
            .ReturnsAsync(nutritionLog);

        // Act
        var result = await _controller.GetNutritionLogByDate(date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLog = Assert.IsType<NutritionLogDto>(okResult.Value);
        Assert.Equal(userId, returnedLog.UserId);
        Assert.Equal(date, returnedLog.LogDate);
    }

    [Fact]
    public async Task GetNutritionLogByDate_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var date = DateTime.UtcNow.Date;

        // Act
        var result = await _controller.GetNutritionLogByDate(date);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetNutritionLogs_WithDateRange_ReturnsFilteredLogs()
    {
        // Arrange
        var userId = 1;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        SetupUserClaims(userId);

        var logs = new List<NutritionLogDto>
        {
            new NutritionLogDto 
            { 
                NutritionLogId = 1, 
                UserId = userId, 
                LogDate = DateTime.UtcNow.AddDays(-3),
                TotalCalories = 2000
            },
            new NutritionLogDto 
            { 
                NutritionLogId = 2, 
                UserId = userId, 
                LogDate = DateTime.UtcNow.AddDays(-1),
                TotalCalories = 1800
            }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetNutritionLogsQuery>(q => 
                q.UserId == userId && 
                q.StartDate == startDate && 
                q.EndDate == endDate),
            default))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetNutritionLogs(startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<NutritionLogDto>>(okResult.Value);
        Assert.Equal(2, returnedLogs.Count());
    }

    [Fact]
    public async Task GetNutritionLogs_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetNutritionLogs(null, null);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AddFoodEntry_WithValidData_ReturnsOkWithId()
    {
        // Arrange
        var userId = 1;
        SetupUserClaims(userId);

        var dto = new AddFoodEntryDto
        {
            FoodItemId = 10,
            Quantity = 150,
            MealType = "Breakfast"
        };

        var foodEntryId = 100;
        _mediatorMock.Setup(m => m.Send(
            It.Is<AddFoodEntryCommand>(c => 
                c.UserId == userId && 
                c.FoodItemId == dto.FoodItemId &&
                c.Quantity == dto.Quantity &&
                c.MealType == dto.MealType),
            default))
            .ReturnsAsync(foodEntryId);

        // Act
        var result = await _controller.AddFoodEntry(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Use reflection to access anonymous type property
        var valueType = okResult.Value.GetType();
        var foodEntryIdProperty = valueType.GetProperty("FoodEntryId");
        Assert.NotNull(foodEntryIdProperty);
        
        var actualFoodEntryId = (int)foodEntryIdProperty.GetValue(okResult.Value)!;
        Assert.Equal(foodEntryId, actualFoodEntryId);
    }

    [Fact]
    public async Task AddFoodEntry_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var dto = new AddFoodEntryDto
        {
            FoodItemId = 10,
            Quantity = 150,
            MealType = "Breakfast"
        };

        // Act
        var result = await _controller.AddFoodEntry(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AddFoodEntry_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not_a_number")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var dto = new AddFoodEntryDto { FoodItemId = 10, Quantity = 150 };

        // Act
        var result = await _controller.AddFoodEntry(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task DeleteFoodEntry_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        var foodEntryId = 100;
        SetupUserClaims(userId);

        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteFoodEntryCommand>(c => 
                c.FoodEntryId == foodEntryId && 
                c.UserId == userId),
            default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFoodEntry(foodEntryId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteFoodEntry_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var foodEntryId = 999;
        SetupUserClaims(userId);

        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteFoodEntryCommand>(c => c.FoodEntryId == foodEntryId),
            default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFoodEntry(foodEntryId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteFoodEntry_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var foodEntryId = 100;

        // Act
        var result = await _controller.DeleteFoodEntry(foodEntryId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateNutritionLog_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var userId = 1;
        SetupUserClaims(userId);

        var dto = new CreateNutritionLogDto
        {
            LogDate = DateTime.UtcNow.Date,
            FoodEntries = new List<CreateFoodEntryDto>
            {
                new CreateFoodEntryDto
                {
                    FoodItemId = 1,
                    Quantity = 200,
                    MealType = "Lunch"
                },
                new CreateFoodEntryDto
                {
                    FoodItemId = 2,
                    Quantity = 150,
                    MealType = "Lunch"
                }
            }
        };

        var nutritionLogId = 50;
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateNutritionLogCommand>(c => c.UserId == userId),
            default))
            .ReturnsAsync(nutritionLogId);

        // Act
        var result = await _controller.CreateNutritionLog(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(NutritionController.GetNutritionLogs), createdResult.ActionName);
        var routeValues = (Microsoft.AspNetCore.Routing.RouteValueDictionary)createdResult.RouteValues!;
        var actualId = (int)routeValues["id"]!;
        Assert.Equal(nutritionLogId, actualId);
    }

    [Fact]
    public async Task CreateNutritionLog_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var dto = new CreateNutritionLogDto
        {
            LogDate = DateTime.UtcNow.Date,
            FoodEntries = new List<CreateFoodEntryDto>()
        };

        // Act
        var result = await _controller.CreateNutritionLog(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateNutritionLog_WithMultipleFoodEntries_CreatesSuccessfully()
    {
        // Arrange
        var userId = 2;
        SetupUserClaims(userId);

        var dto = new CreateNutritionLogDto
        {
            LogDate = DateTime.UtcNow.Date,
            FoodEntries = new List<CreateFoodEntryDto>
            {
                new CreateFoodEntryDto { FoodItemId = 1, Quantity = 100, MealType = "Breakfast" },
                new CreateFoodEntryDto { FoodItemId = 2, Quantity = 150, MealType = "Breakfast" },
                new CreateFoodEntryDto { FoodItemId = 3, Quantity = 200, MealType = "Lunch" }
            }
        };

        var nutritionLogId = 60;
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateNutritionLogCommand>(c => 
                c.UserId == userId && 
                c.NutritionLog.FoodEntries.Count == 3),
            default))
            .ReturnsAsync(nutritionLogId);

        // Act
        var result = await _controller.CreateNutritionLog(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var routeValues = (Microsoft.AspNetCore.Routing.RouteValueDictionary)createdResult.RouteValues!;
        var actualId = (int)routeValues["id"]!;
        Assert.Equal(nutritionLogId, actualId);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateNutritionLogCommand>(c => c.NutritionLog.FoodEntries.Count == 3),
            default), Times.Once);
    }

    [Fact]
    public async Task GetNutritionLogs_WithoutDateRange_ReturnsAllLogs()
    {
        // Arrange
        var userId = 3;
        SetupUserClaims(userId);

        var logs = new List<NutritionLogDto>
        {
            new NutritionLogDto { NutritionLogId = 1, UserId = userId },
            new NutritionLogDto { NutritionLogId = 2, UserId = userId },
            new NutritionLogDto { NutritionLogId = 3, UserId = userId }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetNutritionLogsQuery>(q => 
                q.UserId == userId && 
                q.StartDate == null && 
                q.EndDate == null),
            default))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetNutritionLogs(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<NutritionLogDto>>(okResult.Value);
        Assert.Equal(3, returnedLogs.Count());
    }

    [Fact]
    public async Task GetFoodItems_ReturnsEmptyList_WhenNoItemsExist()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetFoodItemsQuery>(), default))
            .ReturnsAsync(new List<FoodItemDto>());

        // Act
        var result = await _controller.GetFoodItems(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<FoodItemDto>>(okResult.Value);
        Assert.Empty(returnedItems);
    }
}
