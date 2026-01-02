using System;
using System.Collections.Generic;

namespace HealthSync.Application.DTOs
{
    public class AdminDashboardDto
    {
        public DateTime Timestamp { get; set; }
        public KpiStatsDto KpiStats { get; set; } = new();
        public DashboardChartsDto Charts { get; set; } = new();
        public ContentInsightsDto ContentInsights { get; set; } = new();
        public SystemHealthDto SystemHealth { get; set; } = new();
    }

    public class KpiStatsDto
    {
        public KpiValueDto TotalUsers { get; set; } = new();
        public ActiveUsersDto ActiveUsers { get; set; } = new();
        public ContentCountDto ContentCount { get; set; } = new();
        public AiUsageDto AiUsage { get; set; } = new();
    }

    public class KpiValueDto
    {
        public int Value { get; set; }
        public double GrowthRate { get; set; }
        public string Trend { get; set; } = "neutral"; // "up", "down", "neutral"
    }

    public class ActiveUsersDto
    {
        public int Daily { get; set; }
        public int Monthly { get; set; }
        public double GrowthRate { get; set; }
        public string Trend { get; set; } = "neutral";
    }

    public class ContentCountDto
    {
        public int Exercises { get; set; }
        public int FoodItems { get; set; }
        public int Total => Exercises + FoodItems;
    }

    public class AiUsageDto
    {
        public int TotalRequests { get; set; }
        public decimal CostEstimate { get; set; }
        public bool LimitWarning { get; set; }
    }

    public class DashboardChartsDto
    {
        public ChartDataDto UserGrowth { get; set; } = new();
        public PieChartDataDto GoalSuccessRate { get; set; } = new();
        public List<HeatmapPointDto> ActivityHeatmap { get; set; } = new();
    }

    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
        public string Period { get; set; } = "Last 6 Months";
    }

    public class PieChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<double> Data { get; set; } = new(); // Percentages
        public int TotalGoals { get; set; }
    }

    public class HeatmapPointDto
    {
        public int Day { get; set; } // 0-6
        public int Hour { get; set; } // 0-23
        public int Count { get; set; }
    }

    public class ContentInsightsDto
    {
        public List<TopContentItemDto> TopExercises { get; set; } = new();
        public List<TopContentItemDto> TopFoods { get; set; } = new();
        public List<MissedSearchDto> MissedSearches { get; set; } = new();
    }

    public class TopContentItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MissedSearchDto
    {
        public string Keyword { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SystemHealthDto
    {
        public string Status { get; set; } = "Healthy";
        public List<ServiceStatusDto> Services { get; set; } = new();
        public List<ErrorLogDto> RecentErrors { get; set; } = new();
    }

    public class ServiceStatusDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Online";
        public int LatencyMs { get; set; }
    }

    public class ErrorLogDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Code { get; set; }
    }
}
