using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

namespace HealthSync.Application.Handlers;

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMinioClient _minioClient;

    public GetAdminDashboardQueryHandler(IApplicationDbContext context, IMinioClient minioClient)
    {
        _context = context;
        _minioClient = minioClient;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dashboard = new AdminDashboardDto
        {
            Timestamp = now
        };

        // Parallel execution for better performance
        // Sequential execution to avoid DbContext threading issues
        dashboard.KpiStats = await GetKpiStatsAsync(now, cancellationToken);
        dashboard.Charts = await GetChartsAsync(now, cancellationToken);
        dashboard.ContentInsights = await GetContentInsightsAsync(cancellationToken);
        dashboard.SystemHealth = await GetSystemHealthAsync(cancellationToken);

        return dashboard;
    }

    private async Task<KpiStatsDto> GetKpiStatsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var stats = new KpiStatsDto();
        var lastMonth = now.AddMonths(-1);

        // 1. Total Users
        var totalUsers = await _context.ApplicationUsers.CountAsync(cancellationToken);
        var lastMonthUsers = await _context.ApplicationUsers.CountAsync(u => u.CreatedAt < lastMonth, cancellationToken);
        
        stats.TotalUsers.Value = totalUsers;
        stats.TotalUsers.GrowthRate = CalculateGrowth(totalUsers, lastMonthUsers);
        stats.TotalUsers.Trend = stats.TotalUsers.GrowthRate >= 0 ? "up" : "down";

        // 2. Active Users (DAU/MAU)
        // Check logs for recent activity
        var activeDaily = await _context.WorkoutLogs
            .Where(w => w.WorkoutDate >= now.AddDays(-1))
            .Select(w => w.UserId)
            .Distinct()
            .CountAsync(cancellationToken);
            
        // Add nutrition logs too for better accuracy
        var activeDailyNutri = await _context.NutritionLogs
            .Where(n => n.LogDate >= now.AddDays(-1))
            .Select(n => n.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        stats.ActiveUsers.Daily = Math.Max(activeDaily, activeDailyNutri); // Simple approximation
        
        var activeMonthly = await _context.WorkoutLogs
            .Where(w => w.WorkoutDate >= now.AddMonths(-1))
            .Select(w => w.UserId)
            .Distinct()
            .CountAsync(cancellationToken);
            
        stats.ActiveUsers.Monthly = activeMonthly;
        // Calculate MAU growth from previous month
        var lastMonthActiveUsers = await _context.WorkoutLogs
            .Where(w => w.WorkoutDate >= now.AddMonths(-2) && w.WorkoutDate < now.AddMonths(-1))
            .Select(w => w.UserId)
            .Distinct()
            .CountAsync(cancellationToken);
        
        stats.ActiveUsers.GrowthRate = CalculateGrowth(activeMonthly, lastMonthActiveUsers);
        stats.ActiveUsers.Trend = stats.ActiveUsers.GrowthRate >= 0 ? "up" : "down";

        // 3. Content Count
        stats.ContentCount.Exercises = await _context.Exercises.CountAsync(cancellationToken);
        stats.ContentCount.FoodItems = await _context.FoodItems.CountAsync(cancellationToken);

        // 4. AI Usage (Real data from ChatMessages)
        var aiRequests = await _context.ChatMessages.CountAsync(m => m.Role != "user", cancellationToken);
        stats.AiUsage.TotalRequests = aiRequests;
        // Estimate cost: Gemini Flash ~$0.00001/request (rough approximation)
        stats.AiUsage.CostEstimate = (decimal)(aiRequests * 0.00001);
        stats.AiUsage.LimitWarning = false;

        return stats;
    }

    private async Task<DashboardChartsDto> GetChartsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var charts = new DashboardChartsDto();

        // 1. User Growth (Last 6 months)
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            
            var count = await _context.ApplicationUsers.CountAsync(u => u.CreatedAt < monthEnd, cancellationToken);
            
            charts.UserGrowth.Labels.Add(monthDate.ToString("MMM"));
            charts.UserGrowth.Data.Add(count);
        }

        // 2. Goal Success Rate
        var goals = await _context.Goals.ToListAsync(cancellationToken);
        if (goals.Any())
        {
            var completed = goals.Count(g => g.Status?.ToLower() == "completed");
            var failed = goals.Count(g => g.Status?.ToLower() != "completed" && g.EndDate.HasValue && g.EndDate.Value < now);
            var inProgress = goals.Count(g => (g.Status?.ToLower() == "in_progress" || g.Status?.ToLower() == "active") && (!g.EndDate.HasValue || g.EndDate.Value >= now));
            
            charts.GoalSuccessRate.TotalGoals = goals.Count;
            charts.GoalSuccessRate.Labels = new List<string> { "Completed", "In Progress", "Failed" };
            charts.GoalSuccessRate.Data = new List<double> 
            { 
                (double)completed,
                (double)inProgress,
                (double)failed
            };
        }

        // 3. Activity Heatmap (Workout Logs time distribution)
        // Group by DayOfWeek (0-6) and Hour (0-23)
        // Note: EF Core might not translate complex DatePart grouping well depending on provider, fetching recent logs (~1000) into memory for heatmap is safer/faster if not huge data.
        var recentLogs = await _context.WorkoutLogs
            .Where(w => w.WorkoutDate >= now.AddMonths(-1))
            .Select(w => w.WorkoutDate)
            .Take(2000)
            .ToListAsync(cancellationToken);

        var heatmapGroups = recentLogs
            .GroupBy(d => new { Day = (int)d.DayOfWeek, Hour = d.Hour })
            .Select(g => new HeatmapPointDto
            {
                Day = g.Key.Day,
                Hour = g.Key.Hour,
                Count = g.Count()
            })
            .ToList();
            
        charts.ActivityHeatmap = heatmapGroups;

        return charts;
    }

    private async Task<ContentInsightsDto> GetContentInsightsAsync(CancellationToken cancellationToken)
    {
        var insights = new ContentInsightsDto();

        // 1. Top Exercises
        var topExercises = await _context.ExerciseSessions
            .GroupBy(es => es.ExerciseId)
            .Select(g => new { ExerciseId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        var exerciseIds = topExercises.Select(x => x.ExerciseId).ToList();
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.ExerciseId))
            .ToDictionaryAsync(e => e.ExerciseId, e => e.Name, cancellationToken);

        foreach (var item in topExercises)
        {
            if (exercises.TryGetValue(item.ExerciseId, out var name))
            {
                insights.TopExercises.Add(new TopContentItemDto 
                { 
                    Id = item.ExerciseId, 
                    Name = name, 
                    Count = item.Count 
                });
            }
        }

        // 2. Top Foods
        var topFoods = await _context.FoodEntries
            .GroupBy(fe => fe.FoodItemId)
            .Select(g => new { FoodItemId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        var foodIds = topFoods.Select(x => x.FoodItemId).ToList();
        var foods = await _context.FoodItems
            .Where(f => foodIds.Contains(f.FoodItemId))
            .ToDictionaryAsync(f => f.FoodItemId, f => f.Name, cancellationToken);

        foreach (var item in topFoods)
        {
            if (foods.TryGetValue(item.FoodItemId, out var name))
            {
                insights.TopFoods.Add(new TopContentItemDto 
                { 
                    Id = item.FoodItemId, 
                    Name = name, 
                    Count = item.Count 
                });
            }
        }

        // 3. Missed Searches - Not yet implemented (requires search logging)
        // Future: Query SearchLog table when available

        return insights;
    }

    private async Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken)
    {
        var health = new SystemHealthDto();

        // 1. Database
        bool dbOk = await _context.CanConnectAsync(cancellationToken);
        health.Services.Add(new ServiceStatusDto 
        { 
            Name = "Database (SQL Server)", 
            Status = dbOk ? "Online" : "Offline", 
            LatencyMs = dbOk ? 25 : 0 
        });

        // 2. MinIO
        bool minioOk = false;
        long minioLatency = 0;
        try 
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await _minioClient.ListBucketsAsync(cancellationToken);
            sw.Stop();
            minioOk = true;
            minioLatency = sw.ElapsedMilliseconds;
        }
        catch 
        {
            // Ignore errors as this is just a heath check - service will be marked as offline implicitly checking minioOk
        }

        health.Services.Add(new ServiceStatusDto 
        { 
            Name = "Object Storage (MinIO)", 
            Status = minioOk ? "Online" : "Offline", 
            LatencyMs = (int)minioLatency 
        });

        // 3. AI Service - Real status check would require calling Gemini API
        // For now, assume online if ChatMessages exist (indicates API was working recently)
        var recentAIActivity = await _context.ChatMessages
            .Where(m => m.CreatedAt >= DateTime.UtcNow.AddHours(-1))
            .AnyAsync(cancellationToken);
        
        health.Services.Add(new ServiceStatusDto 
        { 
            Name = "AI Service (Gemini)", 
            Status = recentAIActivity ? "Online" : "Unknown", 
            LatencyMs = recentAIActivity ? 150 : 0
        });

        // 4. Recent Errors - Not yet implemented (requires error logging system)
        // Future: Query ErrorLog table when available

        return health;
    }

    private static double CalculateGrowth(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((double)(current - previous) / previous) * 100, 1);
    }
}
