namespace HealthSync.Domain.Constants;

/// <summary>
/// Central registry of all permission codes used throughout the system
/// </summary>
public static class PermissionCodes
{
    // User Management Permissions
    public const string USER_READ = "USER_READ";
    public const string USER_BAN = "USER_BAN";
    public const string USER_UPDATE_ROLE = "USER_UPDATE_ROLE";
    public const string USER_DELETE = "USER_DELETE";
    
    // Exercise Management Permissions
    public const string EXERCISE_READ = "EXERCISE_READ";
    public const string EXERCISE_CREATE = "EXERCISE_CREATE";
    public const string EXERCISE_UPDATE = "EXERCISE_UPDATE";
    public const string EXERCISE_DELETE = "EXERCISE_DELETE";
    
    // Food Management Permissions
    public const string FOOD_READ = "FOOD_READ";
    public const string FOOD_CREATE = "FOOD_CREATE";
    public const string FOOD_UPDATE = "FOOD_UPDATE";
    public const string FOOD_DELETE = "FOOD_DELETE";
    
    // Workout Log Permissions
    public const string WORKOUT_LOG_READ = "WORKOUT_LOG_READ";
    public const string WORKOUT_LOG_CREATE = "WORKOUT_LOG_CREATE";
    public const string WORKOUT_LOG_UPDATE = "WORKOUT_LOG_UPDATE";
    public const string WORKOUT_LOG_DELETE = "WORKOUT_LOG_DELETE";
    
    // Nutrition Log Permissions
    public const string NUTRITION_LOG_READ = "NUTRITION_LOG_READ";
    public const string NUTRITION_LOG_CREATE = "NUTRITION_LOG_CREATE";
    public const string NUTRITION_LOG_UPDATE = "NUTRITION_LOG_UPDATE";
    public const string NUTRITION_LOG_DELETE = "NUTRITION_LOG_DELETE";
    
    // Goal Permissions
    public const string GOAL_READ = "GOAL_READ";
    public const string GOAL_CREATE = "GOAL_CREATE";
    public const string GOAL_UPDATE = "GOAL_UPDATE";
    public const string GOAL_DELETE = "GOAL_DELETE";
    
    // Dashboard Permissions
    public const string DASHBOARD_VIEW = "DASHBOARD_VIEW";
    public const string DASHBOARD_ADMIN = "DASHBOARD_ADMIN";
}
