using HealthSync.Domain.Entities;

namespace HealthSync.Domain.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateJwtToken(ApplicationUser user);
}

public interface IApplicationDbContext
{
    IQueryable<ApplicationUser> ApplicationUsers { get; }
    IQueryable<UserProfile> UserProfiles { get; }
    IQueryable<Goal> Goals { get; }
    IQueryable<ProgressRecord> ProgressRecords { get; }
    IQueryable<WorkoutLog> WorkoutLogs { get; }
    IQueryable<ExerciseSession> ExerciseSessions { get; }
    IQueryable<Exercise> Exercises { get; }
    IQueryable<NutritionLog> NutritionLogs { get; }
    IQueryable<FoodEntry> FoodEntries { get; }
    IQueryable<FoodItem> FoodItems { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<Permission> Permissions { get; }
    IQueryable<RolePermission> RolePermissions { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<ChatMessage> ChatMessages { get; }
    IQueryable<UserActionLog> UserActionLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}