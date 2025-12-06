using System.Security.Claims;

namespace HealthSync.Domain.Constants
{
    /// <summary>
    /// Định nghĩa các vai trò (roles) trong hệ thống HealthSync
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }

    /// <summary>
    /// Định nghĩa các quyền hạn (permissions) trong hệ thống
    /// </summary>
    public static class AppPermissions
    {
        // User Management Permissions
        public const string ViewUsers = "Permissions.Users.View";
        public const string CreateUsers = "Permissions.Users.Create";
        public const string EditUsers = "Permissions.Users.Edit";
        public const string DeleteUsers = "Permissions.Users.Delete";
        public const string ManageUserRoles = "Permissions.Users.ManageRoles";

        // Exercise Management Permissions
        public const string ViewExercises = "Permissions.Exercises.View";
        public const string CreateExercises = "Permissions.Exercises.Create";
        public const string EditExercises = "Permissions.Exercises.Edit";
        public const string DeleteExercises = "Permissions.Exercises.Delete";

        // Food Management Permissions
        public const string ViewFoodItems = "Permissions.FoodItems.View";
        public const string CreateFoodItems = "Permissions.FoodItems.Create";
        public const string EditFoodItems = "Permissions.FoodItems.Edit";
        public const string DeleteFoodItems = "Permissions.FoodItems.Delete";

        // Workout Log Permissions
        public const string ViewOwnWorkoutLogs = "Permissions.WorkoutLogs.ViewOwn";
        public const string CreateOwnWorkoutLogs = "Permissions.WorkoutLogs.CreateOwn";
        public const string EditOwnWorkoutLogs = "Permissions.WorkoutLogs.EditOwn";
        public const string DeleteOwnWorkoutLogs = "Permissions.WorkoutLogs.DeleteOwn";
        public const string ViewAllWorkoutLogs = "Permissions.WorkoutLogs.ViewAll";

        // Nutrition Log Permissions
        public const string ViewOwnNutritionLogs = "Permissions.NutritionLogs.ViewOwn";
        public const string CreateOwnNutritionLogs = "Permissions.NutritionLogs.CreateOwn";
        public const string EditOwnNutritionLogs = "Permissions.NutritionLogs.EditOwn";
        public const string DeleteOwnNutritionLogs = "Permissions.NutritionLogs.DeleteOwn";
        public const string ViewAllNutritionLogs = "Permissions.NutritionLogs.ViewAll";

        // Goal Permissions
        public const string ViewOwnGoals = "Permissions.Goals.ViewOwn";
        public const string CreateOwnGoals = "Permissions.Goals.CreateOwn";
        public const string EditOwnGoals = "Permissions.Goals.EditOwn";
        public const string DeleteOwnGoals = "Permissions.Goals.DeleteOwn";
        public const string ViewAllGoals = "Permissions.Goals.ViewAll";

        // Dashboard & Reports Permissions
        public const string ViewAdminDashboard = "Permissions.Dashboard.ViewAdmin";
        public const string ViewReports = "Permissions.Reports.View";
        public const string ExportReports = "Permissions.Reports.Export";

        // Profile Permissions
        public const string ViewOwnProfile = "Permissions.Profile.ViewOwn";
        public const string EditOwnProfile = "Permissions.Profile.EditOwn";
        public const string ViewAllProfiles = "Permissions.Profile.ViewAll";
    }

    /// <summary>
    /// Định nghĩa các Policy cho Authorization
    /// </summary>
    public static class AppPolicies
    {
        // Admin Policies
        public const string AdminOnly = "AdminOnly";
        public const string CustomerOnly = "CustomerOnly";
        public const string AdminOrCustomer = "AdminOrCustomer";

        // User Management Policies
        public const string CanViewUsers = "CanViewUsers";
        public const string CanManageUsers = "CanManageUsers";
        public const string CanDeleteUsers = "CanDeleteUsers";
        public const string CanManageRoles = "CanManageRoles";

        // Exercise Management Policies
        public const string CanViewExercises = "CanViewExercises";
        public const string CanManageExercises = "CanManageExercises";

        // Food Management Policies
        public const string CanViewFoodItems = "CanViewFoodItems";
        public const string CanManageFoodItems = "CanManageFoodItems";

        // Workout & Nutrition Policies
        public const string CanManageOwnWorkouts = "CanManageOwnWorkouts";
        public const string CanManageOwnNutrition = "CanManageOwnNutrition";
        public const string CanManageOwnGoals = "CanManageOwnGoals";
        public const string CanViewAllData = "CanViewAllData";

        // Dashboard Policies
        public const string CanViewAdminDashboard = "CanViewAdminDashboard";
        public const string CanViewReports = "CanViewReports";
    }

    /// <summary>
    /// Helper để ánh xạ Roles sang Permissions
    /// </summary>
    public static class RolePermissions
    {
        public static readonly Dictionary<string, List<string>> AdminPermissions = new()
        {
            { AppRoles.Admin, new List<string>
                {
                    // All User Management
                    AppPermissions.ViewUsers,
                    AppPermissions.CreateUsers,
                    AppPermissions.EditUsers,
                    AppPermissions.DeleteUsers,
                    AppPermissions.ManageUserRoles,

                    // All Exercise Management
                    AppPermissions.ViewExercises,
                    AppPermissions.CreateExercises,
                    AppPermissions.EditExercises,
                    AppPermissions.DeleteExercises,

                    // All Food Management
                    AppPermissions.ViewFoodItems,
                    AppPermissions.CreateFoodItems,
                    AppPermissions.EditFoodItems,
                    AppPermissions.DeleteFoodItems,

                    // View All Data
                    AppPermissions.ViewAllWorkoutLogs,
                    AppPermissions.ViewAllNutritionLogs,
                    AppPermissions.ViewAllGoals,
                    AppPermissions.ViewAllProfiles,

                    // Dashboard & Reports
                    AppPermissions.ViewAdminDashboard,
                    AppPermissions.ViewReports,
                    AppPermissions.ExportReports,
                }
            }
        };

        public static readonly Dictionary<string, List<string>> CustomerPermissions = new()
        {
            { AppRoles.Customer, new List<string>
                {
                    // View Library
                    AppPermissions.ViewExercises,
                    AppPermissions.ViewFoodItems,

                    // Own Workout Logs
                    AppPermissions.ViewOwnWorkoutLogs,
                    AppPermissions.CreateOwnWorkoutLogs,
                    AppPermissions.EditOwnWorkoutLogs,
                    AppPermissions.DeleteOwnWorkoutLogs,

                    // Own Nutrition Logs
                    AppPermissions.ViewOwnNutritionLogs,
                    AppPermissions.CreateOwnNutritionLogs,
                    AppPermissions.EditOwnNutritionLogs,
                    AppPermissions.DeleteOwnNutritionLogs,

                    // Own Goals
                    AppPermissions.ViewOwnGoals,
                    AppPermissions.CreateOwnGoals,
                    AppPermissions.EditOwnGoals,
                    AppPermissions.DeleteOwnGoals,

                    // Own Profile
                    AppPermissions.ViewOwnProfile,
                    AppPermissions.EditOwnProfile,
                }
            }
        };

        /// <summary>
        /// Lấy danh sách permissions theo role
        /// </summary>
        public static List<string> GetPermissionsForRole(string role)
        {
            return role switch
            {
                AppRoles.Admin => AdminPermissions[AppRoles.Admin],
                AppRoles.Customer => CustomerPermissions[AppRoles.Customer],
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Tạo Claims cho user dựa trên role
        /// </summary>
        public static List<Claim> GetClaimsForRole(string role)
        {
            var permissions = GetPermissionsForRole(role);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            return claims;
        }
    }
}
