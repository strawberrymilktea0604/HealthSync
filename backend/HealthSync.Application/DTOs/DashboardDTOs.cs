using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthSync.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalWorkoutLogs { get; set; }
        public int TotalNutritionLogs { get; set; }
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
    }
}