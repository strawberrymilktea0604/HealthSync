using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
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

    IQueryable<ApplicationUser> IApplicationDbContext.ApplicationUsers => ApplicationUsers;
    IQueryable<UserProfile> IApplicationDbContext.UserProfiles => UserProfiles;

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
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
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
    }
}