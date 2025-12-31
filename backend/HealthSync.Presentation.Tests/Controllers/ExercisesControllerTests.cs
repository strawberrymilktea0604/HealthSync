using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class ExercisesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IStorageService> _storageServiceMock;

    private readonly ExercisesController _controller;

    public ExercisesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _storageServiceMock = new Mock<IStorageService>();
        _controller = new ExercisesController(_mediatorMock.Object, _storageServiceMock.Object);
    }

    [Fact]
    public async Task GetExercises_WithoutFilters_ReturnsAllExercises()
    {
        // Arrange
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest" },
            new ExerciseDto { ExerciseId = 2, Name = "Squat", MuscleGroup = "Legs" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetExercisesQuery>(), default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Equal(2, returnedExercises.Count());
    }

    [Fact]
    public async Task GetExercises_WithMuscleGroupFilter_ReturnsFilteredExercises()
    {
        // Arrange
        var muscleGroup = "Chest";
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest" }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExercisesQuery>(q => q.MuscleGroup == muscleGroup),
            default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(muscleGroup, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Single(returnedExercises);
    }

    [Fact]
    public async Task GetExercises_WithSearchTerm_ReturnsMatchingExercises()
    {
        // Arrange
        var search = "Press";
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest" },
            new ExerciseDto { ExerciseId = 3, Name = "Shoulder Press", MuscleGroup = "Shoulders" }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExercisesQuery>(q => q.Search == search),
            default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(null, null, search);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Equal(2, returnedExercises.Count());
    }

    [Fact]
    public async Task GetExerciseById_ExerciseExists_ReturnsExercise()
    {
        // Arrange
        var exerciseId = 1;
        var exercise = new ExerciseDto 
        { 
            ExerciseId = exerciseId, 
            Name = "Bench Press", 
            MuscleGroup = "Chest",
            Difficulty = "Intermediate"
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExerciseByIdQuery>(q => q.ExerciseId == exerciseId),
            default))
            .ReturnsAsync(exercise);

        // Act
        var result = await _controller.GetExerciseById(exerciseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercise = Assert.IsType<ExerciseDto>(okResult.Value);
        Assert.Equal(exerciseId, returnedExercise.ExerciseId);
        Assert.Equal("Bench Press", returnedExercise.Name);
    }

    [Fact]
    public async Task GetExerciseById_ExerciseNotExists_ReturnsNotFound()
    {
        // Arrange
        var exerciseId = 999;
        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExerciseByIdQuery>(q => q.ExerciseId == exerciseId),
            default))
            .ReturnsAsync((ExerciseDto?)null);

        // Act
        var result = await _controller.GetExerciseById(exerciseId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        dynamic? value = notFoundResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task CreateExercise_ValidCommand_ReturnsCreatedResult()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Deadlift",
            MuscleGroup = "Back",
            Difficulty = "Advanced",
            Equipment = "Barbell"
        };
        var exerciseId = 10;

        _mediatorMock.Setup(m => m.Send(command, default))
            .ReturnsAsync(exerciseId);

        // Act
        var result = await _controller.CreateExercise(command);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ExercisesController.GetExerciseById), createdResult.ActionName);
        var routeValues = (Microsoft.AspNetCore.Routing.RouteValueDictionary)createdResult.RouteValues!;
        var actualId = (int)routeValues["id"]!;
        Assert.Equal(exerciseId, actualId);
    }

    [Fact]
    public async Task CreateExercise_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateExerciseCommand();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateExercise(command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateExercise_ValidCommand_ReturnsOkResult()
    {
        // Arrange
        var exerciseId = 1;
        var command = new UpdateExerciseCommand
        {
            ExerciseId = exerciseId,
            Name = "Updated Bench Press",
            MuscleGroup = "Chest",
            Difficulty = "Advanced"
        };

        _mediatorMock.Setup(m => m.Send(command, default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateExercise(exerciseId, command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic? value = okResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task UpdateExercise_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var urlId = 1;
        var command = new UpdateExerciseCommand
        {
            ExerciseId = 2, // Different from URL
            Name = "Exercise"
        };

        // Act
        var result = await _controller.UpdateExercise(urlId, command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        dynamic? value = badRequestResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task UpdateExercise_ExerciseNotExists_ReturnsNotFound()
    {
        // Arrange
        var exerciseId = 999;
        var command = new UpdateExerciseCommand
        {
            ExerciseId = exerciseId,
            Name = "Non-existent Exercise"
        };

        _mediatorMock.Setup(m => m.Send(command, default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateExercise(exerciseId, command);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        dynamic? value = notFoundResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task UpdateExercise_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var exerciseId = 1;
        var command = new UpdateExerciseCommand { ExerciseId = exerciseId };
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateExercise(exerciseId, command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteExercise_ExerciseExists_ReturnsOkResult()
    {
        // Arrange
        var exerciseId = 1;
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteExerciseCommand>(c => c.ExerciseId == exerciseId),
            default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteExercise(exerciseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic? value = okResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task DeleteExercise_ExerciseNotExists_ReturnsNotFound()
    {
        // Arrange
        var exerciseId = 999;
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteExerciseCommand>(c => c.ExerciseId == exerciseId),
            default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteExercise(exerciseId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        dynamic? value = notFoundResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task DeleteExercise_ThrowsInvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var exerciseId = 1;
        var errorMessage = "Cannot delete exercise that is used in workout logs";
        
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteExerciseCommand>(c => c.ExerciseId == exerciseId),
            default))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.DeleteExercise(exerciseId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        dynamic? value = badRequestResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task GetExercises_WithMultipleFilters_ReturnsFilteredExercises()
    {
        // Arrange
        var muscleGroup = "Chest";
        var difficulty = "Beginner";
        var search = "Press";
        
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto 
            { 
                ExerciseId = 1, 
                Name = "Incline Press", 
                MuscleGroup = "Chest",
                Difficulty = "Beginner"
            }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExercisesQuery>(q => 
                q.MuscleGroup == muscleGroup && 
                q.Difficulty == difficulty && 
                q.Search == search),
            default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(muscleGroup, difficulty, search);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Single(returnedExercises);
        
        var exercise = returnedExercises.First();
        Assert.Equal("Incline Press", exercise.Name);
        Assert.Equal("Chest", exercise.MuscleGroup);
        Assert.Equal("Beginner", exercise.Difficulty);
    }
}
