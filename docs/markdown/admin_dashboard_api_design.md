# Admin Dashboard API Design

## Endpoint
`GET /api/admin/dashboard`

## Purpose
Aggregates all key metrics for the Admin Dashboard in a single optimized call (or split into micro-calls if preferred, but single call is faster for initial load).

## Response JSON Structure

```json
{
  "timestamp": "2024-05-20T10:30:00Z",
  "kpiStats": {
    "totalUsers": {
      "value": 1250,
      "growthRate": 5.2,
      "trend": "up" // or "down", "neutral"
    },
    "activeUsers": {
      "daily": 350,
      "monthly": 980,
      "growthRate": 12.5,
      "trend": "up"
    },
    "contentCount": {
      "exercises": 45,
      "foodItems": 120,
      "total": 165
    },
    "aiUsage": {
      "totalRequests": 5432,
      "costEstimate": 12.50, // in USD or Tokens
      "limitWarning": false
    }
  },
  "charts": {
    "userGrowth": {
      "labels": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
      "data": [100, 150, 220, 310, 400, 520],
      "period": "Last 6 Months"
    },
    "goalSuccessRate": {
      "labels": ["Completed", "In Progress", "Failed"],
      "data": [45, 40, 15], // Percentages
      "totalGoals": 850
    },
    "activityHeatmap": [
      // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
      // hour = 0-23
      { "day": 1, "hour": 6, "count": 45 }, 
      { "day": 1, "hour": 7, "count": 80 },
      { "day": 1, "hour": 18, "count": 120 },
      // ... meaningful data points only
    ]
  },
  "contentInsights": {
    "topExercises": [
      { "id": 12, "name": "Bench Press", "count": 1500 },
      { "id": 5, "name": "Squat", "count": 1350 },
      { "id": 8, "name": "Deadlift", "count": 900 },
      { "id": 3, "name": "Plank", "count": 850 },
      { "id": 1, "name": "Push-up", "count": 600 }
    ],
    "topFoods": [
      { "id": 101, "name": "Chicken Breast", "count": 2100 },
      { "id": 105, "name": "White Rice", "count": 1800 },
      { "id": 102, "name": "Egg", "count": 1500 },
      { "id": 150, "name": "Banana", "count": 1200 },
      { "id": 201, "name": "Whey Protein", "count": 1100 }
    ],
    "missedSearches": [
      { "keyword": "Keto Bread", "count": 45 },
      { "keyword": "Muscle Up", "count": 32 },
      { "keyword": "Vegan Pizza", "count": 28 }
    ]
  },
  "systemHealth": {
    "status": "Healthy", // or "Degraded", "Critical"
    "services": [
      { "name": "Database (SQL Server)", "status": "Online", "latencyMs": 45 },
      { "name": "Object Storage (MinIO)", "status": "Online", "latencyMs": 120 },
      { "name": "AI Service (Gemini)", "status": "Online", "latencyMs": 850 }
    ],
    "recentErrors": [
      { 
        "id": "err-123", 
        "timestamp": "2024-05-20T09:15:00Z", 
        "message": "Gateway Timeout from Gemini API", 
        "code": 504 
      },
      { 
        "id": "err-124", 
        "timestamp": "2024-05-20T08:30:00Z", 
        "message": "NullReferenceException in WorkoutLogService", 
        "code": 500 
      }
    ]
  }
}
```

## Implementation Strategy (Backend)

1.  **DTO Classes**: Create a `AdminDashboardDto` class that mirrors this structure.
2.  **Query Handler**: `GetAdminDashboardQueryHandler` will:
    *   Execute concurrent tasks to fetch independent data sections (Parallel Execution).
    *   Use `MemoryCache` (10-15 mins expiry) for heavy stats (like User Growth or Heatmap).
    *   Use direct fast queries for "Real-time" stats (like Status).
3.  **Controller**: simple `[HttpGet("dashboard")]` endpoint returning `ActionResult<AdminDashboardDto>`.
