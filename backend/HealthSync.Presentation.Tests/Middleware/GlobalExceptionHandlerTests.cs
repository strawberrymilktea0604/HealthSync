using HealthSync.Presentation.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Net;
using System.Text.Json;
using Xunit;

namespace HealthSync.Presentation.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _middleware;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        RequestDelegate next = (HttpContext context) => Task.CompletedTask;
        _middleware = new GlobalExceptionHandler(next, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext_WhenNoException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;

        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandler(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode); // Default status
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleUnauthorizedAccessException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Explicitly set MemoryStream
        var exception = new UnauthorizedAccessException("Access denied");

        RequestDelegate next = (ctx) =>
        {
            throw exception;
        };

        var middleware = new GlobalExceptionHandler(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        Assert.Contains("Access denied", responseBody);
        Assert.Contains("401", responseBody);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleInvalidOperationException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Invalid operation");

        RequestDelegate next = (ctx) =>
        {
            throw exception;
        };

        var middleware = new GlobalExceptionHandler(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        Assert.Contains("Invalid operation", responseBody);
        Assert.Contains("400", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleArgumentException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("Invalid argument");

        RequestDelegate next = (ctx) =>
        {
            throw exception;
        };

        var middleware = new GlobalExceptionHandler(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        Assert.Contains("Invalid argument", responseBody);
        Assert.Contains("400", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleGenericException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Something went wrong");

        RequestDelegate next = (ctx) =>
        {
            throw exception;
        };

        var middleware = new GlobalExceptionHandler(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        // Deserialize the JSON response
        var response = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseBody);
        var errorMessage = response.GetProperty("Error").GetString();
        var statusCode = response.GetProperty("StatusCode").GetInt32();
        
        Assert.Equal("Đã xảy ra lỗi nội bộ", errorMessage);
        Assert.Equal(500, statusCode);
    }
}