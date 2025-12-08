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

        // Get Profile
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile != null)
        {
            var age = DateTime.Now.Year - profile.Dob.Year;
            var bmr = CalculateBMR(profile.Gender, profile.WeightKg, profile.HeightCm, age);

            context.Profile = new ProfileContextDto
            {
                Gender = profile.Gender,
                Age = age,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.WeightKg,
                Bmr = bmr,
                ActivityLevel = profile.ActivityLevel
            };
        }

        // Get Active Goal
        var goal = await _context.Goals
            .AsNoTracking()
            .Include(g => g.ProgressRecords)
            .Where(g => g.UserId == userId && g.Status == "in_progress")
            .OrderByDescending(g => g.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (goal != null)
        {
            var latestProgress = goal.ProgressRecords
                .OrderByDescending(p => p.RecordDate)
                .FirstOrDefault();

            context.Goal = new GoalContextDto
            {
                Type = goal.Type,
                TargetWeightKg = goal.TargetValue,
                Deadline = goal.EndDate
            };

            if (latestProgress != null)
            {
                context.Profile.CurrentWeightKg = latestProgress.WeightKg;
            }
        }

        // Get Last 7 Days Logs
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
        var today = DateTime.UtcNow.Date;

        var nutritionLogs = await _context.NutritionLogs
            .AsNoTracking()
            .Include(n => n.FoodEntries)
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

            var nutritionLog = nutritionLogs.FirstOrDefault(n => n.LogDate.Date == date);
            if (nutritionLog != null && nutritionLog.FoodEntries.Any())
            {
                dailyLog.Nutrition = new NutritionContextDto
                {
                    Calories = nutritionLog.FoodEntries.Sum(f => f.CaloriesKcal ?? 0),
                    ProteinG = nutritionLog.FoodEntries.Sum(f => f.ProteinG ?? 0),
                    CarbsG = nutritionLog.FoodEntries.Sum(f => f.CarbsG ?? 0),
                    FatG = nutritionLog.FoodEntries.Sum(f => f.FatG ?? 0)
                };
            }

            var workoutLog = workoutLogs.FirstOrDefault(w => w.WorkoutDate.Date == date);
            if (workoutLog != null)
            {
                dailyLog.Workout = new WorkoutContextDto
                {
                    Status = "Completed",
                    DurationMin = workoutLog.DurationMin,
                    Focus = workoutLog.ExerciseSessions
                        .Select(es => es.Exercise.MuscleGroup)
                        .Distinct()
                        .ToList(),
                    Notes = workoutLog.Notes
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
}
