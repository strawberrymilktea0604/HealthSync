using HealthSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Infrastructure.Persistence;

public class HealthSyncDbContext : DbContext
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
            entity.Property(e => e.Role).IsRequired();
        });

        // --- UserProfile (One-to-One with ApplicationUser) ---
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId); // PK is also the FK
            entity.HasOne(e => e.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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
                .HasForeignKey(e => e.WorkoutLogId);
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.ExerciseSessions)
                .HasForeignKey(e => e.ExerciseId);
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
        });

        // --- FoodEntry (Many-to-Many between NutritionLog and FoodItem) ---
        modelBuilder.Entity<FoodEntry>(entity =>
        {
            entity.HasKey(e => e.FoodEntryId);
            entity.HasOne(e => e.NutritionLog)
                .WithMany(nl => nl.FoodEntries)
                .HasForeignKey(e => e.NutritionLogId);
            entity.HasOne(e => e.FoodItem)
                .WithMany(fi => fi.FoodEntries)
                .HasForeignKey(e => e.FoodItemId);
        });

        // --- FoodItem ---
        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodItemId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // --- Goal (Many-to-One with ApplicationUser) ---
        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- ProgressRecord (Many-to-One with Goal) ---
        modelBuilder.Entity<ProgressRecord>(entity =>
        {
            entity.HasKey(e => e.ProgressRecordId);
            entity.HasOne(e => e.Goal)
                .WithMany(g => g.ProgressRecords)
                .HasForeignKey(e => e.GoalId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}