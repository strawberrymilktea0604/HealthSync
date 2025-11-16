using FluentValidation;
using HealthSync.Application.Commands;

namespace HealthSync.Application.Validators;

public class CreateWorkoutLogCommandValidator : AbstractValidator<CreateWorkoutLogCommand>
{
    public CreateWorkoutLogCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.WorkoutLog.WorkoutDate)
            .NotEmpty().WithMessage("WorkoutDate is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("WorkoutDate cannot be in the future.");

        RuleFor(x => x.WorkoutLog.DurationMin)
            .GreaterThan(0).WithMessage("DurationMin must be greater than 0.")
            .LessThanOrEqualTo(1440).WithMessage("DurationMin cannot exceed 1440 minutes (24 hours).");

        RuleFor(x => x.WorkoutLog.ExerciseSessions)
            .NotEmpty().WithMessage("At least one exercise session is required.");

        RuleForEach(x => x.WorkoutLog.ExerciseSessions)
            .SetValidator(new CreateExerciseSessionDtoValidator());
    }
}

public class CreateExerciseSessionDtoValidator : AbstractValidator<DTOs.CreateExerciseSessionDto>
{
    public CreateExerciseSessionDtoValidator()
    {
        RuleFor(x => x.ExerciseId)
            .GreaterThan(0).WithMessage("ExerciseId must be greater than 0.");

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("Sets must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Sets cannot exceed 100.");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Reps cannot exceed 1000.");

        RuleFor(x => x.WeightKg)
            .GreaterThanOrEqualTo(0).WithMessage("WeightKg must be greater than or equal to 0.")
            .LessThanOrEqualTo(1000).WithMessage("WeightKg cannot exceed 1000 kg.");

        RuleFor(x => x.RestSec)
            .GreaterThanOrEqualTo(0).When(x => x.RestSec.HasValue)
            .WithMessage("RestSec must be greater than or equal to 0.")
            .LessThanOrEqualTo(3600).When(x => x.RestSec.HasValue)
            .WithMessage("RestSec cannot exceed 3600 seconds (1 hour).");

        RuleFor(x => x.Rpe)
            .InclusiveBetween(1, 10).When(x => x.Rpe.HasValue)
            .WithMessage("Rpe must be between 1 and 10.");
    }
}