namespace HealthSync.Domain.Constants;

/// <summary>
/// Central registry of all authorization policies in the system
/// </summary>
public static class Policies
{
    // Role-based policies
    public const string ADMIN_ONLY = "AdminOnly";
    public const string CUSTOMER_ONLY = "CustomerOnly";
    public const string ADMIN_OR_CUSTOMER = "AdminOrCustomer";
    
    // User Management Policies
    public const string CAN_VIEW_USERS = "CanViewUsers";
    public const string CAN_MANAGE_USERS = "CanManageUsers";
    public const string CAN_BAN_USERS = "CanBanUsers";
    public const string CAN_UPDATE_USER_ROLES = "CanUpdateUserRoles";
    public const string CAN_DELETE_USERS = "CanDeleteUsers";
    
    // Exercise Management Policies
    public const string CAN_VIEW_EXERCISES = "CanViewExercises";
    public const string CAN_CREATE_EXERCISES = "CanCreateExercises";
    public const string CAN_UPDATE_EXERCISES = "CanUpdateExercises";
    public const string CAN_DELETE_EXERCISES = "CanDeleteExercises";
    public const string CAN_MANAGE_EXERCISES = "CanManageExercises";
    
    // Food Management Policies
    public const string CAN_VIEW_FOODS = "CanViewFoods";
    public const string CAN_CREATE_FOODS = "CanCreateFoods";
    public const string CAN_UPDATE_FOODS = "CanUpdateFoods";
    public const string CAN_DELETE_FOODS = "CanDeleteFoods";
    public const string CAN_MANAGE_FOODS = "CanManageFoods";
    
    // Workout Log Policies
    public const string CAN_VIEW_OWN_WORKOUT_LOGS = "CanViewOwnWorkoutLogs";
    public const string CAN_CREATE_OWN_WORKOUT_LOGS = "CanCreateOwnWorkoutLogs";
    public const string CAN_UPDATE_OWN_WORKOUT_LOGS = "CanUpdateOwnWorkoutLogs";
    public const string CAN_DELETE_OWN_WORKOUT_LOGS = "CanDeleteOwnWorkoutLogs";
    public const string CAN_MANAGE_OWN_WORKOUT_LOGS = "CanManageOwnWorkoutLogs";
    public const string CAN_VIEW_ALL_WORKOUT_LOGS = "CanViewAllWorkoutLogs";
    
    // Nutrition Log Policies
    public const string CAN_VIEW_OWN_NUTRITION_LOGS = "CanViewOwnNutritionLogs";
    public const string CAN_CREATE_OWN_NUTRITION_LOGS = "CanCreateOwnNutritionLogs";
    public const string CAN_UPDATE_OWN_NUTRITION_LOGS = "CanUpdateOwnNutritionLogs";
    public const string CAN_DELETE_OWN_NUTRITION_LOGS = "CanDeleteOwnNutritionLogs";
    public const string CAN_MANAGE_OWN_NUTRITION_LOGS = "CanManageOwnNutritionLogs";
    public const string CAN_VIEW_ALL_NUTRITION_LOGS = "CanViewAllNutritionLogs";
    
    // Goal Policies
    public const string CAN_VIEW_OWN_GOALS = "CanViewOwnGoals";
    public const string CAN_CREATE_OWN_GOALS = "CanCreateOwnGoals";
    public const string CAN_UPDATE_OWN_GOALS = "CanUpdateOwnGoals";
    public const string CAN_DELETE_OWN_GOALS = "CanDeleteOwnGoals";
    public const string CAN_MANAGE_OWN_GOALS = "CanManageOwnGoals";
    public const string CAN_VIEW_ALL_GOALS = "CanViewAllGoals";
    
    // Profile Policies
    public const string CAN_VIEW_OWN_PROFILE = "CanViewOwnProfile";
    public const string CAN_UPDATE_OWN_PROFILE = "CanUpdateOwnProfile";
    public const string CAN_VIEW_ALL_PROFILES = "CanViewAllProfiles";
    
    // Dashboard & Reports Policies
    public const string CAN_VIEW_ADMIN_DASHBOARD = "CanViewAdminDashboard";
    public const string CAN_VIEW_REPORTS = "CanViewReports";
    public const string CAN_EXPORT_REPORTS = "CanExportReports";
}
