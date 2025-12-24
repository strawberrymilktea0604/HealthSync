using System.Security.Claims;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthSync.Presentation.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ChatController>> _loggerMock;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ChatController>>();
        _controller = new ChatController(_mediatorMock.Object, _loggerMock.Object);

        // Setup User Claims
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Customer")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task AskHealthBot_WithValidQuestion_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new ChatRequestDto { Question = "How to lose weight?" };
        var expectedResponse = new ChatResponseDto
        {
            Response = "Here are some tips for weight loss...",
            Timestamp = DateTime.UtcNow,
            MessageId = Guid.NewGuid()
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ChatWithBotQuery>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChatResponseDto>(okResult.Value);
        Assert.Equal(expectedResponse.Response, response.Response);
        Assert.Equal(expectedResponse.MessageId, response.MessageId);
    }

    [Fact]
    public async Task AskHealthBot_WithEmptyQuestion_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChatRequestDto { Question = "" };

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task AskHealthBot_WithWhitespaceQuestion_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChatRequestDto { Question = "   " };

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task AskHealthBot_WithInvalidUserId_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ChatRequestDto { Question = "Test question" };
        
        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid"),
            new Claim(ClaimTypes.Role, "Customer")
        }, "TestAuth"));

        _controller.ControllerContext.HttpContext.User = invalidUser;

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task AskHealthBot_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ChatRequestDto { Question = "Test question" };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ChatWithBotQuery>(), default))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetChatHistory_WithValidParameters_ReturnsOkWithHistory()
    {
        // Arrange
        var expectedHistory = new List<ChatHistoryDto>
        {
            new ChatHistoryDto
            {
                MessageId = Guid.NewGuid(),
                Role = "user",
                Content = "How to lose weight?",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new ChatHistoryDto
            {
                MessageId = Guid.NewGuid(),
                Role = "assistant",
                Content = "Here are some tips...",
                CreatedAt = DateTime.UtcNow.AddMinutes(-9)
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChatHistoryQuery>(), default))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetChatHistory(20, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var history = Assert.IsAssignableFrom<List<ChatHistoryDto>>(okResult.Value);
        Assert.Equal(2, history.Count);
        Assert.Equal("user", history[0].Role);
        Assert.Equal("assistant", history[1].Role);
    }

    [Fact]
    public async Task GetChatHistory_WithExcessivePageSize_LimitsTo50()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetChatHistoryQuery>(q => q.PageSize == 50), default))
            .ReturnsAsync(new List<ChatHistoryDto>());

        // Act
        var result = await _controller.GetChatHistory(100, 1);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.Is<GetChatHistoryQuery>(q => q.PageSize == 50), default), Times.Once);
    }

    [Fact]
    public async Task GetChatHistory_WithInvalidUserId_ReturnsUnauthorized()
    {
        // Arrange
        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ""),
            new Claim(ClaimTypes.Role, "Customer")
        }, "TestAuth"));

        _controller.ControllerContext.HttpContext.User = invalidUser;

        // Act
        var result = await _controller.GetChatHistory(20, 1);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetChatHistory_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChatHistoryQuery>(), default))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetChatHistory(20, 1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task AskHealthBot_WithNullQuestion_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChatRequestDto { Question = null! };

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task AskHealthBot_WithMissingUserId_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ChatRequestDto { Question = "Test question" };
        
        var userWithoutId = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Customer")
        }, "TestAuth"));

        _controller.ControllerContext.HttpContext.User = userWithoutId;

        // Act
        var result = await _controller.AskHealthBot(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetChatHistory_WithNegativePageNumber_UsesPageOne()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetChatHistoryQuery>(q => q.PageNumber == 1), default))
            .ReturnsAsync(new List<ChatHistoryDto>());

        // Act
        var result = await _controller.GetChatHistory(20, -5);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.Is<GetChatHistoryQuery>(q => q.PageNumber == 1), default), Times.Once);
    }

    [Fact]
    public async Task GetChatHistory_WithZeroPageNumber_UsesPageOne()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetChatHistoryQuery>(q => q.PageNumber == 1), default))
            .ReturnsAsync(new List<ChatHistoryDto>());

        // Act
        var result = await _controller.GetChatHistory(20, 0);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.Is<GetChatHistoryQuery>(q => q.PageNumber == 1), default), Times.Once);
    }

    [Fact]
    public async Task GetChatHistory_WithDefaultParameters_UsesDefaults()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetChatHistoryQuery>(q => 
                q.PageSize == 20 && q.PageNumber == 1), default))
            .ReturnsAsync(new List<ChatHistoryDto>());

        // Act
        var result = await _controller.GetChatHistory();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.Is<GetChatHistoryQuery>(q => 
            q.PageSize == 20 && q.PageNumber == 1), default), Times.Once);
    }

    [Fact]
    public async Task GetChatHistory_WithMissingUserId_ReturnsUnauthorized()
    {
        // Arrange
        var userWithoutId = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Customer")
        }, "TestAuth"));

        _controller.ControllerContext.HttpContext.User = userWithoutId;

        // Act
        var result = await _controller.GetChatHistory(20, 1);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public void HealthCheck_ShouldReturnHealthyStatus()
    {
        // Act
        var result = _controller.HealthCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value;
        var statusProp = response.GetType().GetProperty("status");
        var serviceProp = response.GetType().GetProperty("service");
        var timestampProp = response.GetType().GetProperty("timestamp");
        
        Assert.Equal("healthy", statusProp?.GetValue(response));
        Assert.Equal("HealthSync Chatbot", serviceProp?.GetValue(response));
        Assert.NotNull(timestampProp?.GetValue(response));
    }
}
