using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Infrastructure.Persistence;

public class HealthSyncDbContext : DbContext, IApplicationDbContext
{
    public HealthSyncDbContext(DbContextOptions<HealthSyncDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<ProgressRecord> ProgressRecords { get; set; }
    public DbSet<WorkoutLog> WorkoutLogs { get; set; }
    public DbSet<ExerciseSession> ExerciseSessions { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<NutritionLog> NutritionLogs { get; set; }
    public DbSet<FoodEntry> FoodEntries { get; set; }
    public DbSet<FoodItem> FoodItems { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    IQueryable<ApplicationUser> IApplicationDbContext.ApplicationUsers => ApplicationUsers;
    IQueryable<UserProfile> IApplicationDbContext.UserProfiles => UserProfiles;
    IQueryable<Goal> IApplicationDbContext.Goals => Goals;
    IQueryable<ProgressRecord> IApplicationDbContext.ProgressRecords => ProgressRecords;
    IQueryable<WorkoutLog> IApplicationDbContext.WorkoutLogs => WorkoutLogs;
    IQueryable<ExerciseSession> IApplicationDbContext.ExerciseSessions => ExerciseSessions;
    IQueryable<Exercise> IApplicationDbContext.Exercises => Exercises;
    IQueryable<NutritionLog> IApplicationDbContext.NutritionLogs => NutritionLogs;
    IQueryable<FoodEntry> IApplicationDbContext.FoodEntries => FoodEntries;
    IQueryable<FoodItem> IApplicationDbContext.FoodItems => FoodItems;
    IQueryable<Role> IApplicationDbContext.Roles => Roles;
    IQueryable<Permission> IApplicationDbContext.Permissions => Permissions;
    IQueryable<RolePermission> IApplicationDbContext.RolePermissions => RolePermissions;
    IQueryable<UserRole> IApplicationDbContext.UserRoles => UserRoles;
    IQueryable<ChatMessage> IApplicationDbContext.ChatMessages => ChatMessages;

    void IApplicationDbContext.Add<T>(T entity)
    {
        Add(entity);
    }

    void IApplicationDbContext.Update<T>(T entity)
    {
        Update(entity);
    }

    void IApplicationDbContext.Remove<T>(T entity)
    {
        Remove(entity);
    }

    Task<bool> IApplicationDbContext.CanConnectAsync(CancellationToken cancellationToken)
    {
        return Database.CanConnectAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HealthSyncDbContext).Assembly);

        // --- ApplicationUser ---
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
        });

        // --- UserProfile (One-to-One with ApplicationUser) ---
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId); // PK is also the FK
            entity.HasOne(e => e.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.HeightCm).HasPrecision(5, 2);
            entity.Property(e => e.WeightKg).HasPrecision(5, 2);
        });

        // --- WorkoutLog (Many-to-One with ApplicationUser) ---
        modelBuilder.Entity<WorkoutLog>(entity =>
        {
            entity.HasKey(e => e.WorkoutLogId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.WorkoutLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- ExerciseSession (Many-to-Many between WorkoutLog and Exercise) ---
        modelBuilder.Entity<ExerciseSession>(entity =>
        {
            entity.HasKey(e => e.ExerciseSessionId);
            entity.HasOne(e => e.WorkoutLog)
                .WithMany(wl => wl.ExerciseSessions)
                .HasForeignKey(e => e.WorkoutLogId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.ExerciseSessions)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.WeightKg).HasPrecision(5, 2);
            entity.Property(e => e.Rpe).HasPrecision(4, 2);
        });

        // --- Exercise ---
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // --- NutritionLog (Many-to-One with ApplicationUser) ---
        modelBuilder.Entity<NutritionLog>(entity =>
        {
            entity.HasKey(e => e.NutritionLogId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.NutritionLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.TotalCalories).HasPrecision(8, 2);
            entity.Property(e => e.ProteinG).HasPrecision(8, 2);
            entity.Property(e => e.CarbsG).HasPrecision(8, 2);
            entity.Property(e => e.FatG).HasPrecision(8, 2);
        });

        // --- FoodEntry (Many-to-Many between NutritionLog and FoodItem) ---
        modelBuilder.Entity<FoodEntry>(entity =>
        {
            entity.HasKey(e => e.FoodEntryId);
            entity.HasOne(e => e.NutritionLog)
                .WithMany(nl => nl.FoodEntries)
                .HasForeignKey(e => e.NutritionLogId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FoodItem)
                .WithMany(fi => fi.FoodEntries)
                .HasForeignKey(e => e.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Quantity).HasPrecision(8, 2);
            entity.Property(e => e.CaloriesKcal).HasPrecision(8, 2);
            entity.Property(e => e.ProteinG).HasPrecision(8, 2);
            entity.Property(e => e.CarbsG).HasPrecision(8, 2);
            entity.Property(e => e.FatG).HasPrecision(8, 2);
        });

        // --- FoodItem ---
        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodItemId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.ServingSize).HasPrecision(8, 2);
            entity.Property(e => e.CaloriesKcal).HasPrecision(8, 2);
            entity.Property(e => e.ProteinG).HasPrecision(8, 2);
            entity.Property(e => e.CarbsG).HasPrecision(8, 2);
            entity.Property(e => e.FatG).HasPrecision(8, 2);
        });

        // --- Goal (Many-to-One with ApplicationUser) ---
        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.TargetValue).HasPrecision(8, 2);
        });

        // --- ProgressRecord (Many-to-One with Goal) ---
        modelBuilder.Entity<ProgressRecord>(entity =>
        {
            entity.HasKey(e => e.ProgressRecordId);
            entity.HasOne(e => e.Goal)
                .WithMany(g => g.ProgressRecords)
                .HasForeignKey(e => e.GoalId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Value).HasPrecision(8, 2);
            entity.Property(e => e.WeightKg).HasPrecision(5, 2);
            entity.Property(e => e.WaistCm).HasPrecision(5, 2);
        });

        // --- Role ---
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        // --- Permission ---
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PermissionCode).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.PermissionCode).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        // --- RolePermission (Many-to-Many between Role and Permission) ---
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- UserRole (Many-to-Many between ApplicationUser and Role) ---
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- ChatMessage ---
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.ChatMessageId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // Seed data for FoodItems
        modelBuilder.Entity<FoodItem>().HasData(
            new FoodItem { FoodItemId = 1, Name = "Chicken Breast", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 165, ProteinG = 31, CarbsG = 0, FatG = 3.6m },
            new FoodItem { FoodItemId = 2, Name = "Brown Rice", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 111, ProteinG = 2.6m, CarbsG = 23, FatG = 0.9m },
            new FoodItem { FoodItemId = 3, Name = "Banana", ServingSize = 118, ServingUnit = "g", CaloriesKcal = 105, ProteinG = 1.3m, CarbsG = 27, FatG = 0.4m },
            new FoodItem { FoodItemId = 4, Name = "Greek Yogurt", ServingSize = 170, ServingUnit = "g", CaloriesKcal = 100, ProteinG = 17, CarbsG = 6, FatG = 0 },
            new FoodItem { FoodItemId = 5, Name = "Spinach", ServingSize = 30, ServingUnit = "g", CaloriesKcal = 7, ProteinG = 0.9m, CarbsG = 1.1m, FatG = 0.1m },
            new FoodItem { FoodItemId = 6, Name = "Salmon", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 208, ProteinG = 20, CarbsG = 0, FatG = 13m },
            new FoodItem { FoodItemId = 7, Name = "Sweet Potato", ServingSize = 130, ServingUnit = "g", CaloriesKcal = 112, ProteinG = 2.1m, CarbsG = 26, FatG = 0.1m },
            new FoodItem { FoodItemId = 8, Name = "Eggs", ServingSize = 50, ServingUnit = "g", CaloriesKcal = 72, ProteinG = 6.3m, CarbsG = 0.4m, FatG = 4.8m },
            new FoodItem { FoodItemId = 9, Name = "Oatmeal", ServingSize = 40, ServingUnit = "g", CaloriesKcal = 150, ProteinG = 5.3m, CarbsG = 27, FatG = 2.8m },
            new FoodItem { FoodItemId = 10, Name = "Broccoli", ServingSize = 91, ServingUnit = "g", CaloriesKcal = 31, ProteinG = 2.5m, CarbsG = 6, FatG = 0.3m }
        );

        // Seed data for Exercises
        modelBuilder.Entity<Exercise>().HasData(
            // Chest Exercises
            new Exercise { ExerciseId = 1, Name = "Push-ups", MuscleGroup = SystemConstants.MuscleGroups.Chest, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.None, Description = "Classic bodyweight chest exercise" },
            new Exercise { ExerciseId = 2, Name = "Bench Press", MuscleGroup = SystemConstants.MuscleGroups.Chest, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Barbell, Description = "Compound chest exercise with barbell" },
            new Exercise { ExerciseId = 3, Name = "Dumbbell Fly", MuscleGroup = SystemConstants.MuscleGroups.Chest, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Isolation exercise for chest" },
            
            // Back Exercises
            new Exercise { ExerciseId = 4, Name = "Pull-ups", MuscleGroup = SystemConstants.MuscleGroups.Back, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.PullUpBar, Description = "Bodyweight back exercise" },
            new Exercise { ExerciseId = 5, Name = "Deadlift", MuscleGroup = SystemConstants.MuscleGroups.Back, Difficulty = SystemConstants.Difficulty.Advanced, Equipment = SystemConstants.Equipment.Barbell, Description = "Compound full-body exercise" },
            new Exercise { ExerciseId = 6, Name = "Bent-over Row", MuscleGroup = SystemConstants.MuscleGroups.Back, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Barbell, Description = "Compound back exercise" },
            
            // Legs Exercises
            new Exercise { ExerciseId = 7, Name = "Squats", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.None, Description = "Fundamental leg exercise" },
            new Exercise { ExerciseId = 8, Name = "Lunges", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.None, Description = "Unilateral leg exercise" },
            new Exercise { ExerciseId = 9, Name = "Leg Press", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Machine, Description = "Machine-based leg exercise" },
            
            // Shoulders Exercises
            new Exercise { ExerciseId = 10, Name = "Shoulder Press", MuscleGroup = SystemConstants.MuscleGroups.Shoulders, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Overhead pressing movement" },
            new Exercise { ExerciseId = 11, Name = "Lateral Raise", MuscleGroup = SystemConstants.MuscleGroups.Shoulders, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Isolation shoulder exercise" },
            
            // Arms Exercises
            new Exercise { ExerciseId = 12, Name = "Bicep Curls", MuscleGroup = SystemConstants.MuscleGroups.Arms, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Isolation bicep exercise" },
            new Exercise { ExerciseId = 13, Name = "Tricep Dips", MuscleGroup = SystemConstants.MuscleGroups.Arms, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.ParallelBars, Description = "Bodyweight tricep exercise" },
            
            // Core Exercises
            new Exercise { ExerciseId = 14, Name = "Plank", MuscleGroup = SystemConstants.MuscleGroups.Core, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.None, Description = "Isometric core exercise" },
            new Exercise { ExerciseId = 15, Name = "Crunches", MuscleGroup = SystemConstants.MuscleGroups.Core, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.None, Description = "Basic abdominal exercise" }
        );

        // Seed data for Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, RoleName = RoleNames.ADMIN, Description = "Quản trị viên hệ thống, có toàn quyền" },
            new Role { Id = 2, RoleName = RoleNames.CUSTOMER, Description = "Người dùng cuối sử dụng app" }
        );

        // Seed data for Permissions
        modelBuilder.Entity<Permission>().HasData(
            // User Management
            new Permission { Id = 101, PermissionCode = "USER_READ", Description = "Xem danh sách người dùng", Category = SystemConstants.Categories.User },
            new Permission { Id = 102, PermissionCode = "USER_BAN", Description = "Khóa tài khoản người dùng", Category = SystemConstants.Categories.User },
            new Permission { Id = 103, PermissionCode = "USER_UPDATE_ROLE", Description = "Cập nhật vai trò người dùng", Category = SystemConstants.Categories.User },
            new Permission { Id = 104, PermissionCode = "USER_DELETE", Description = "Xóa người dùng", Category = SystemConstants.Categories.User },
            
            // Exercise Management
            new Permission { Id = 201, PermissionCode = "EXERCISE_READ", Description = "Xem thư viện bài tập", Category = SystemConstants.Categories.Exercise },
            new Permission { Id = 202, PermissionCode = "EXERCISE_CREATE", Description = "Thêm bài tập mới", Category = SystemConstants.Categories.Exercise },
            new Permission { Id = 203, PermissionCode = "EXERCISE_UPDATE", Description = "Cập nhật bài tập", Category = SystemConstants.Categories.Exercise },
            new Permission { Id = 204, PermissionCode = "EXERCISE_DELETE", Description = "Xóa bài tập", Category = SystemConstants.Categories.Exercise },
            
            // Food Management
            new Permission { Id = 301, PermissionCode = "FOOD_READ", Description = "Xem thư viện thực phẩm", Category = SystemConstants.Categories.Food },
            new Permission { Id = 302, PermissionCode = "FOOD_CREATE", Description = "Thêm thực phẩm mới", Category = SystemConstants.Categories.Food },
            new Permission { Id = 303, PermissionCode = "FOOD_UPDATE", Description = "Cập nhật thực phẩm", Category = SystemConstants.Categories.Food },
            new Permission { Id = 304, PermissionCode = "FOOD_DELETE", Description = "Xóa thực phẩm", Category = SystemConstants.Categories.Food },
            
            // Workout Log
            new Permission { Id = 401, PermissionCode = "WORKOUT_LOG_READ", Description = "Xem nhật ký tập luyện", Category = SystemConstants.Categories.WorkoutLog },
            new Permission { Id = 402, PermissionCode = "WORKOUT_LOG_CREATE", Description = "Tạo nhật ký tập luyện", Category = SystemConstants.Categories.WorkoutLog },
            new Permission { Id = 403, PermissionCode = "WORKOUT_LOG_UPDATE", Description = "Cập nhật nhật ký tập luyện", Category = SystemConstants.Categories.WorkoutLog },
            new Permission { Id = 404, PermissionCode = "WORKOUT_LOG_DELETE", Description = "Xóa nhật ký tập luyện", Category = SystemConstants.Categories.WorkoutLog },
            
            // Nutrition Log
            new Permission { Id = 501, PermissionCode = "NUTRITION_LOG_READ", Description = "Xem nhật ký dinh dưỡng", Category = SystemConstants.Categories.NutritionLog },
            new Permission { Id = 502, PermissionCode = "NUTRITION_LOG_CREATE", Description = "Tạo nhật ký dinh dưỡng", Category = SystemConstants.Categories.NutritionLog },
            new Permission { Id = 503, PermissionCode = "NUTRITION_LOG_UPDATE", Description = "Cập nhật nhật ký dinh dưỡng", Category = SystemConstants.Categories.NutritionLog },
            new Permission { Id = 504, PermissionCode = "NUTRITION_LOG_DELETE", Description = "Xóa nhật ký dinh dưỡng", Category = SystemConstants.Categories.NutritionLog },
            
            // Goal Management
            new Permission { Id = 601, PermissionCode = "GOAL_READ", Description = "Xem mục tiêu", Category = SystemConstants.Categories.Goal },
            new Permission { Id = 602, PermissionCode = "GOAL_CREATE", Description = "Tạo mục tiêu", Category = SystemConstants.Categories.Goal },
            new Permission { Id = 603, PermissionCode = "GOAL_UPDATE", Description = "Cập nhật mục tiêu", Category = SystemConstants.Categories.Goal },
            new Permission { Id = 604, PermissionCode = "GOAL_DELETE", Description = "Xóa mục tiêu", Category = SystemConstants.Categories.Goal },
            
            // Dashboard
            new Permission { Id = 701, PermissionCode = "DASHBOARD_VIEW", Description = "Xem dashboard cá nhân", Category = SystemConstants.Categories.Dashboard },
            new Permission { Id = 702, PermissionCode = "DASHBOARD_ADMIN", Description = "Xem dashboard admin", Category = SystemConstants.Categories.Dashboard }
        );

        // Seed data for RolePermissions
        modelBuilder.Entity<RolePermission>().HasData(
            // Admin permissions - Full access to everything
            new RolePermission { RoleId = 1, PermissionId = 101 }, // USER_READ
            new RolePermission { RoleId = 1, PermissionId = 102 }, // USER_BAN
            new RolePermission { RoleId = 1, PermissionId = 103 }, // USER_UPDATE_ROLE
            new RolePermission { RoleId = 1, PermissionId = 104 }, // USER_DELETE
            new RolePermission { RoleId = 1, PermissionId = 201 }, // EXERCISE_READ
            new RolePermission { RoleId = 1, PermissionId = 202 }, // EXERCISE_CREATE
            new RolePermission { RoleId = 1, PermissionId = 203 }, // EXERCISE_UPDATE
            new RolePermission { RoleId = 1, PermissionId = 204 }, // EXERCISE_DELETE
            new RolePermission { RoleId = 1, PermissionId = 301 }, // FOOD_READ
            new RolePermission { RoleId = 1, PermissionId = 302 }, // FOOD_CREATE
            new RolePermission { RoleId = 1, PermissionId = 303 }, // FOOD_UPDATE
            new RolePermission { RoleId = 1, PermissionId = 304 }, // FOOD_DELETE
            new RolePermission { RoleId = 1, PermissionId = 401 }, // WORKOUT_LOG_READ
            new RolePermission { RoleId = 1, PermissionId = 402 }, // WORKOUT_LOG_CREATE
            new RolePermission { RoleId = 1, PermissionId = 403 }, // WORKOUT_LOG_UPDATE
            new RolePermission { RoleId = 1, PermissionId = 404 }, // WORKOUT_LOG_DELETE
            new RolePermission { RoleId = 1, PermissionId = 501 }, // NUTRITION_LOG_READ
            new RolePermission { RoleId = 1, PermissionId = 502 }, // NUTRITION_LOG_CREATE
            new RolePermission { RoleId = 1, PermissionId = 503 }, // NUTRITION_LOG_UPDATE
            new RolePermission { RoleId = 1, PermissionId = 504 }, // NUTRITION_LOG_DELETE
            new RolePermission { RoleId = 1, PermissionId = 601 }, // GOAL_READ
            new RolePermission { RoleId = 1, PermissionId = 602 }, // GOAL_CREATE
            new RolePermission { RoleId = 1, PermissionId = 603 }, // GOAL_UPDATE
            new RolePermission { RoleId = 1, PermissionId = 604 }, // GOAL_DELETE
            new RolePermission { RoleId = 1, PermissionId = 701 }, // DASHBOARD_VIEW
            new RolePermission { RoleId = 1, PermissionId = 702 }, // DASHBOARD_ADMIN
            
            // Customer permissions - Limited to personal data management
            new RolePermission { RoleId = 2, PermissionId = 201 }, // EXERCISE_READ (can view exercise library)
            new RolePermission { RoleId = 2, PermissionId = 301 }, // FOOD_READ (can view food library)
            new RolePermission { RoleId = 2, PermissionId = 401 }, // WORKOUT_LOG_READ
            new RolePermission { RoleId = 2, PermissionId = 402 }, // WORKOUT_LOG_CREATE
            new RolePermission { RoleId = 2, PermissionId = 403 }, // WORKOUT_LOG_UPDATE
            new RolePermission { RoleId = 2, PermissionId = 404 }, // WORKOUT_LOG_DELETE
            new RolePermission { RoleId = 2, PermissionId = 501 }, // NUTRITION_LOG_READ
            new RolePermission { RoleId = 2, PermissionId = 502 }, // NUTRITION_LOG_CREATE
            new RolePermission { RoleId = 2, PermissionId = 503 }, // NUTRITION_LOG_UPDATE
            new RolePermission { RoleId = 2, PermissionId = 504 }, // NUTRITION_LOG_DELETE
            new RolePermission { RoleId = 2, PermissionId = 601 }, // GOAL_READ
            new RolePermission { RoleId = 2, PermissionId = 602 }, // GOAL_CREATE
            new RolePermission { RoleId = 2, PermissionId = 603 }, // GOAL_UPDATE
            new RolePermission { RoleId = 2, PermissionId = 604 }, // GOAL_DELETE
            new RolePermission { RoleId = 2, PermissionId = 701 }  // DASHBOARD_VIEW
        );
    }
}