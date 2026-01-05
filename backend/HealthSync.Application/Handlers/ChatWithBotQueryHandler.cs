using MediatR;
using Microsoft.EntityFrameworkCore;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using System.Text.Json;

namespace HealthSync.Application.Handlers;

public class ChatWithBotQueryHandler : IRequestHandler<ChatWithBotQuery, ChatResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAiChatService _aiChatService;

    public ChatWithBotQueryHandler(IApplicationDbContext context, IAiChatService aiChatService)
    {
        _context = context;
        _aiChatService = aiChatService;
    }

    public async Task<ChatResponseDto> Handle(ChatWithBotQuery request, CancellationToken cancellationToken)
    {
        // 0. Check if user exists (to prevent FK violation or processing for deleted user)
        var userExists = await _context.ApplicationUsers.AnyAsync(u => u.UserId == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new UnauthorizedAccessException("User account no longer exists or has been deleted.");
        }

        // 1. Aggregate user context data
        var userContext = await BuildUserContextAsync(request.UserId, cancellationToken);
        var contextJson = JsonSerializer.Serialize(userContext, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true 
        });

        // 2. Save user message
        var userMessage = new ChatMessage
        {
            ChatMessageId = Guid.NewGuid(),
            UserId = request.UserId,
            Role = "user",
            Content = request.Question,
            CreatedAt = DateTime.UtcNow,
            ContextData = contextJson
        };
        _context.Add(userMessage);

        // 3. Call AI service
        var aiResponse = await _aiChatService.GetHealthAdviceAsync(contextJson, request.Question, cancellationToken);

        // 4. Save AI response
        var assistantMessage = new ChatMessage
        {
            ChatMessageId = Guid.NewGuid(),
            UserId = request.UserId,
            Role = "assistant",
            Content = aiResponse,
            CreatedAt = DateTime.UtcNow
        };
        _context.Add(assistantMessage);

        await _context.SaveChangesAsync(cancellationToken);

        return new ChatResponseDto
        {
            Response = aiResponse,
            Timestamp = assistantMessage.CreatedAt,
            MessageId = assistantMessage.ChatMessageId
        };
    }

    private async Task<UserContextDto> BuildUserContextAsync(int userId, CancellationToken cancellationToken)
    {
        var context = new UserContextDto();

        // 1. Get Profile
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile != null)
        {
            var age = DateTime.Now.Year - profile.Dob.Year;
            var bmr = CalculateBMR(profile.Gender, profile.WeightKg, profile.HeightCm, age);
            var bmi = CalculateBMI(profile.WeightKg, profile.HeightCm);
            var bmiStatus = GetBMIStatus(bmi);

            context.Profile = new ProfileContextDto
            {
                Gender = profile.Gender,
                Age = age,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.WeightKg,
                Bmr = bmr,
                Bmi = bmi,
                BmiStatus = bmiStatus,
                ActivityLevel = profile.ActivityLevel
            };
        }

        // 2. Get Goals (Active & Completed)
        var allGoals = await _context.Goals
            .AsNoTracking()
            .Include(g => g.ProgressRecords)
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.StartDate)
            .ToListAsync(cancellationToken);

        // Find Active Goal ("in_progress" or "active")
        var activeGoal = allGoals.FirstOrDefault(g => g.Status == "in_progress" || g.Status == "active");
        if (activeGoal != null)
        {
            var latestProgress = activeGoal.ProgressRecords
                .OrderByDescending(p => p.RecordDate)
                .FirstOrDefault();

            context.Goal = new GoalContextDto
            {
                Type = activeGoal.Type,
                TargetWeightKg = activeGoal.TargetValue,
                Deadline = activeGoal.EndDate,
                Status = activeGoal.Status,
                CurrentProgress = latestProgress?.WeightKg ?? context.Profile.CurrentWeightKg // Fallback to current weight
            };

            // Update profile current weight if progress is newer
            if (latestProgress != null)
            {
                context.Profile.CurrentWeightKg = latestProgress.WeightKg;
            }
        }

        // Get Completed Goals for motivation
        var completedGoals = allGoals
            .Where(g => g.Status == "completed")
            .OrderByDescending(g => g.EndDate ?? g.StartDate)
            .Take(5)
            .Select(g => $"{g.Type}: {g.TargetValue}kg (Done at {g.EndDate:dd/MM/yyyy})")
            .ToList();
        
        context.CompletedGoalsHistory = completedGoals;

        // 3. Get Data Warehouse Logs (User Actions)
        var recentActions = await _context.UserActionLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(20) 
            .Select(a => new { a.Timestamp, a.ActionType, a.Description })
            .ToListAsync(cancellationToken);

        if (recentActions.Any())
        {
            context.RecentActivityLogs = string.Join("\n", recentActions.Select(a => 
                $"- [{a.Timestamp:dd/MM HH:mm}] {a.Description}"));
        }

        // 4. Get System Resources (Foods & Exercises) for Suggestions
        // Limit to top 30 common items to save context
        context.AvailableFoodsSummary = await _context.FoodItems
            .AsNoTracking()
            .OrderBy(f => f.FoodItemId) // Or order by popularity if available
            .Take(40)
            .Select(f => $"{f.Name} ({f.CaloriesKcal}kcal/{f.ServingSize}{f.ServingUnit})")
            .ToListAsync(cancellationToken);

        context.AvailableExercisesSummary = await _context.Exercises
            .AsNoTracking()
            .OrderBy(e => e.ExerciseId)
            .Take(40)
            .Select(e => $"{e.Name} ({e.MuscleGroup}) - {e.Difficulty}")
            .ToListAsync(cancellationToken);

        // 5. Get Last 7 Days Logs (Detailed)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
        var today = DateTime.UtcNow.Date;

        var nutritionLogs = await _context.NutritionLogs
            .AsNoTracking()
            .Include(n => n.FoodEntries)
                .ThenInclude(fe => fe.FoodItem)
            .Where(n => n.UserId == userId && n.LogDate >= sevenDaysAgo && n.LogDate <= today)
            .ToListAsync(cancellationToken);

        var workoutLogs = await _context.WorkoutLogs
            .AsNoTracking()
            .Include(w => w.ExerciseSessions)
                .ThenInclude(es => es.Exercise)
            .Where(w => w.UserId == userId && w.WorkoutDate >= sevenDaysAgo && w.WorkoutDate <= today)
            .ToListAsync(cancellationToken);

        // Build daily logs
        for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
        {
            var dailyLog = new DailyLogContextDto { Date = date };

            // Nutrition
            var dailyNutrition = nutritionLogs.Where(n => n.LogDate.Date == date).ToList();
            if (dailyNutrition.Any())
            {
                var allEntries = dailyNutrition.SelectMany(n => n.FoodEntries).ToList();
                dailyLog.Nutrition = new NutritionContextDto
                {
                    Calories = allEntries.Sum(f => f.CaloriesKcal ?? 0),
                    ProteinG = allEntries.Sum(f => f.ProteinG ?? 0),
                    CarbsG = allEntries.Sum(f => f.CarbsG ?? 0),
                    FatG = allEntries.Sum(f => f.FatG ?? 0),
                    FoodItems = allEntries.Select(f => f.FoodItem?.Name ?? "Unknown").Distinct().ToList()
                };
            }

            // Workout
            var dailyWorkout = workoutLogs.Where(w => w.WorkoutDate.Date == date).ToList();
            if (dailyWorkout.Any())
            {
                var allSessions = dailyWorkout.SelectMany(w => w.ExerciseSessions).ToList();
                dailyLog.Workout = new WorkoutContextDto
                {
                    Status = "Completed",
                    DurationMin = dailyWorkout.Sum(w => w.DurationMin),
                    Focus = allSessions
                        .Select(es => es.Exercise?.MuscleGroup ?? "General")
                        .Distinct()
                        .ToList(),
                    Exercises = allSessions
                        .Select(es => es.Exercise?.Name ?? "Unknown")
                        .Distinct()
                        .ToList(),
                    Notes = string.Join("; ", dailyWorkout.Where(w => !string.IsNullOrEmpty(w.Notes)).Select(w => w.Notes))
                };
            }
            else
            {
                dailyLog.Workout = new WorkoutContextDto { Status = "Rest" };
            }

            context.RecentLogsLast7Days.Add(dailyLog);
        }

        return context;
    }

    private static decimal CalculateBMR(string gender, decimal weightKg, decimal heightCm, int age)
    {
        // Mifflin-St Jeor Equation
        if (gender.ToLower() == "male")
        {
            return 10 * weightKg + 6.25m * heightCm - 5 * age + 5;
        }
        else
        {
            return 10 * weightKg + 6.25m * heightCm - 5 * age - 161;
        }
    }

    private static decimal CalculateBMI(decimal weightKg, decimal heightCm)
    {
        // BMI = weight(kg) / (height(m))^2
        var heightM = heightCm / 100;
        return weightKg / (heightM * heightM);
    }

    private static string GetBMIStatus(decimal bmi)
    {
        if (bmi < 18.5m) return "Thiếu cân";
        if (bmi < 25m) return "Bình thường";
        if (bmi < 30m) return "Thừa cân";
        return "Béo phì";
    }
}
