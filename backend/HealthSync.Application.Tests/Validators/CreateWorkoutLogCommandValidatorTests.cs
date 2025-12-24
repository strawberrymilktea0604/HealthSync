using FluentValidation.TestHelper;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Validators;

namespace HealthSync.Application.Tests.Validators;

public class CreateWorkoutLogCommandValidatorTests
{
    private readonly CreateWorkoutLogCommandValidator _validator;

    public CreateWorkoutLogCommandValidatorTests()
    {
        _validator = new CreateWorkoutLogCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now.AddHours(-1),
                DurationMin = 60,
                Notes = "Good workout",
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = 50,
                        RestSec = 60
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_UserIdZero_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 0,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto { ExerciseId = 1, Sets = 3, Reps = 10 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be greater than 0.");
    }

    [Fact]
    public void Validate_WorkoutDateInFuture_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now.AddDays(1),
                DurationMin = 60,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto { ExerciseId = 1, Sets = 3, Reps = 10 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutLog.WorkoutDate)
            .WithErrorMessage("WorkoutDate cannot be in the future.");
    }

    [Fact]
    public void Validate_DurationMinZero_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 0,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto { ExerciseId = 1, Sets = 3, Reps = 10 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutLog.DurationMin)
            .WithErrorMessage("DurationMin must be greater than 0.");
    }

    [Fact]
    public void Validate_DurationMinExceedsLimit_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 1500, // More than 24 hours
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto { ExerciseId = 1, Sets = 3, Reps = 10 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutLog.DurationMin)
            .WithErrorMessage("DurationMin cannot exceed 1440 minutes (24 hours).");
    }

    [Fact]
    public void Validate_EmptyExerciseSessions_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                ExerciseSessions = new List<CreateExerciseSessionDto>()
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutLog.ExerciseSessions)
            .WithErrorMessage("At least one exercise session is required.");
    }

    [Fact]
    public void Validate_ExerciseSessionWithZeroSets_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 0,
                        Reps = 10,
                        WeightKg = 50
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("WorkoutLog.ExerciseSessions[0].Sets")
            .WithErrorMessage("Sets must be greater than 0.");
    }

    [Fact]
    public void Validate_ExerciseSessionWithZeroReps_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 0,
                        WeightKg = 50
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("WorkoutLog.ExerciseSessions[0].Reps")
            .WithErrorMessage("Reps must be greater than 0.");
    }

    [Fact]
    public void Validate_ExerciseSessionWithNegativeWeight_FailsValidation()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = -5
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("WorkoutLog.ExerciseSessions[0].WeightKg")
            .WithErrorMessage("WeightKg must be greater than or equal to 0.");
    }

    [Fact]
    public void Validate_MultipleExerciseSessions_ValidatesAll()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now.AddHours(-1),
                DurationMin = 90,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = 50
                    },
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 2,
                        Sets = 4,
                        Reps = 12,
                        WeightKg = 30
                    }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
