using Bogus;
using HealthSync.Domain.Constants;
using HealthSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HealthSync.Infrastructure.Persistence;

/// <summary>
/// Initializes the database with seed data for development and testing.
/// Includes robust seeding for Users, Workouts, Nutrition, Goals, etc.
/// </summary>
public static class DbInitializer
{
    private static readonly Faker _faker = new Faker("vi"); // Use Vietnamese locale for realistic data

    public static void SeedData(HealthSyncDbContext context)
    {
        // 0. Ensure database connection
        if (!context.Database.CanConnect()) return;

        // 1. Seed Master Data (Exercises & FoodItems)
        SeedExercises(context);
        SeedFoodItems(context);

        // 2. Seed Transactional Data (Users and their history)
        SeedUsersAndData(context);
    }

    private static void SeedExercises(HealthSyncDbContext context)
    {
        // Check if we already have a good amount of exercises
        if (context.Exercises.Count() >= 30) return;

        var existingNames = context.Exercises.Select(e => e.Name).ToHashSet();
        
        var exercises = new List<Exercise>
        {
            // Chest
            new() { Name = "Incline Bench Press", MuscleGroup = SystemConstants.MuscleGroups.Chest, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Barbell, Description = "Upper chest focus" },
            new() { Name = "Cable Crossover", MuscleGroup = SystemConstants.MuscleGroups.Chest, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Machine, Description = "Isolation for chest" },
            new() { Name = "Dips", MuscleGroup = SystemConstants.MuscleGroups.Chest, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.ParallelBars, Description = "Lower chest and triceps" },
            
            // Back
            new() { Name = "Lat Pulldown", MuscleGroup = SystemConstants.MuscleGroups.Back, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Machine, Description = "Vertical pulling for back width" },
            new() { Name = "Seated Cable Row", MuscleGroup = SystemConstants.MuscleGroups.Back, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Machine, Description = "Horizontal pulling for back thickness" },
            new() { Name = "Face Pull", MuscleGroup = SystemConstants.MuscleGroups.Back, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Machine, Description = "Rear delts and rotator cuff" },
            
            // Legs
            new() { Name = "Romanian Deadlift", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Barbell, Description = "Hamstring and glute focus" },
            new() { Name = "Leg Extension", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Machine, Description = "Isolation for quadriceps" },
            new() { Name = "Leg Curl", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Machine, Description = "Isolation for hamstrings" },
            new() { Name = "Calf Raise", MuscleGroup = SystemConstants.MuscleGroups.Legs, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Machine, Description = "Calf muscle isolation" },

            // Shoulders
            new() { Name = "Arnold Press", MuscleGroup = SystemConstants.MuscleGroups.Shoulders, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Rotational shoulder press" },
            new() { Name = "Front Raise", MuscleGroup = SystemConstants.MuscleGroups.Shoulders, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Front delt isolation" },

            // Arms
            new() { Name = "Tricep Pushdown", MuscleGroup = SystemConstants.MuscleGroups.Arms, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Machine, Description = "Tricep isolation" },
            new() { Name = "Hammer Curl", MuscleGroup = SystemConstants.MuscleGroups.Arms, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.Dumbbells, Description = "Bicep and forearm focus" },
            new() { Name = "Skull Crushers", MuscleGroup = SystemConstants.MuscleGroups.Arms, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.Barbell, Description = "Tricep power exercise" },

            // Core
            new() { Name = "Russian Twist", MuscleGroup = SystemConstants.MuscleGroups.Core, Difficulty = SystemConstants.Difficulty.Beginner, Equipment = SystemConstants.Equipment.None, Description = "Oblique focus" },
            new() { Name = "Leg Raise", MuscleGroup = SystemConstants.MuscleGroups.Core, Difficulty = SystemConstants.Difficulty.Intermediate, Equipment = SystemConstants.Equipment.PullUpBar, Description = "Lower abs focus" }
        };

        var toAdd = exercises.Where(e => !existingNames.Contains(e.Name)).ToList();
        
        if (toAdd.Any())
        {
            context.Exercises.AddRange(toAdd);
            context.SaveChanges();
        }
    }

    private static void SeedFoodItems(HealthSyncDbContext context)
    {
        if (context.FoodItems.Count() >= 50) return;

        var existingNames = context.FoodItems.Select(f => f.Name).ToHashSet();

        var foodItems = new List<FoodItem>
        {
            // Proteins
            new() { Name = "Beef Steak", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 250, ProteinG = 26, CarbsG = 0, FatG = 15 },
            new() { Name = "Ground Beef (Lean)", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 200, ProteinG = 24, CarbsG = 0, FatG = 10 },
            new() { Name = "Turkey Breast", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 135, ProteinG = 30, CarbsG = 0, FatG = 1 },
            new() { Name = "Tuna (Canned)", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 116, ProteinG = 26, CarbsG = 0, FatG = 1 },
            new() { Name = "Cottage Cheese", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 98, ProteinG = 11, CarbsG = 3.4m, FatG = 4.3m },
            new() { Name = "Tofu", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 76, ProteinG = 8, CarbsG = 1.9m, FatG = 4.8m },

            // Carbs
            new() { Name = "White Rice", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 130, ProteinG = 2.7m, CarbsG = 28, FatG = 0.3m },
            new() { Name = "Quinoa", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 120, ProteinG = 4.4m, CarbsG = 21, FatG = 1.9m },
            new() { Name = "Pasta", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 131, ProteinG = 5, CarbsG = 25, FatG = 1.1m },
            new() { Name = "Whole Wheat Bread", ServingSize = 50, ServingUnit = "g", CaloriesKcal = 130, ProteinG = 6, CarbsG = 23, FatG = 2 },
            new() { Name = "Potato", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 77, ProteinG = 2, CarbsG = 17, FatG = 0.1m },
            
            // Fruits
            new() { Name = "Apple", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 52, ProteinG = 0.3m, CarbsG = 14, FatG = 0.2m },
            new() { Name = "Orange", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 47, ProteinG = 0.9m, CarbsG = 12, FatG = 0.1m },
            new() { Name = "Blueberries", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 57, ProteinG = 0.7m, CarbsG = 14, FatG = 0.3m },
            new() { Name = "Strawberries", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 32, ProteinG = 0.7m, CarbsG = 7.7m, FatG = 0.3m },
            new() { Name = "Avocado", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 160, ProteinG = 2, CarbsG = 8.5m, FatG = 15 },

            // Vegetables
            new() { Name = "Carrot", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 41, ProteinG = 0.9m, CarbsG = 10, FatG = 0.2m },
            new() { Name = "Cucumber", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 15, ProteinG = 0.7m, CarbsG = 3.6m, FatG = 0.1m },
            new() { Name = "Tomato", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 18, ProteinG = 0.9m, CarbsG = 3.9m, FatG = 0.2m },
            new() { Name = "Bell Pepper", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 20, ProteinG = 0.9m, CarbsG = 4.6m, FatG = 0.2m },
            new() { Name = "Kale", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 49, ProteinG = 4.3m, CarbsG = 8.8m, FatG = 0.9m },

            // Fats & Others
            new() { Name = "Almonds", ServingSize = 30, ServingUnit = "g", CaloriesKcal = 170, ProteinG = 6, CarbsG = 6, FatG = 15 },
            new() { Name = "Peanut Butter", ServingSize = 30, ServingUnit = "g", CaloriesKcal = 188, ProteinG = 7, CarbsG = 7, FatG = 16 },
            new() { Name = "Olive Oil", ServingSize = 15, ServingUnit = "ml", CaloriesKcal = 119, ProteinG = 0, CarbsG = 0, FatG = 13.5m },
            new() { Name = "Milk (Whole)", ServingSize = 250, ServingUnit = "ml", CaloriesKcal = 150, ProteinG = 8, CarbsG = 12, FatG = 8 },
            new() { Name = "Whey Protein", ServingSize = 30, ServingUnit = "g", CaloriesKcal = 120, ProteinG = 24, CarbsG = 3, FatG = 1 }
        };

        var toAdd = foodItems.Where(f => !existingNames.Contains(f.Name)).ToList();

        if (toAdd.Any())
        {
            context.FoodItems.AddRange(toAdd);
            context.SaveChanges();
        }
    }

    private static void SeedUsersAndData(HealthSyncDbContext context)
    {
        // Require at least 50 users
        if (context.ApplicationUsers.Count() >= 50) return;

        int usersToCreate = 50 - context.ApplicationUsers.Count();
        
        // Fetch All necessary IDs for random picking
        // Load FoodItems into memory for lookups
        var exerciseIds = context.Exercises.Select(e => e.ExerciseId).ToList();
        var foodDict = context.FoodItems.ToDictionary(f => f.FoodItemId, f => f);
        var foodItemIds = foodDict.Keys.ToList();
        
        var customerRole = context.Roles.FirstOrDefault(r => r.RoleName == "Customer"); 

        if (customerRole == null) return; 

        var passwordHash = HashPassword("Password123!");
        
        var users = new List<ApplicationUser>();

        // Generate Users
        for (int i = 0; i < usersToCreate; i++)
        {
            var gender = _faker.PickRandom("Male", "Female");
            var firstName = _faker.Name.FirstName(gender == "Male" ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female);
            var lastName = _faker.Name.LastName(gender == "Male" ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female);
            var email = _faker.Internet.Email(firstName, lastName);

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = _faker.Date.Past(1),
            };
            
            users.Add(user);
        }

        context.ApplicationUsers.AddRange(users);
        context.SaveChanges();

        // Now process each user to add roles and transaction data
        foreach (var user in users)
        {
            // 1. Assign Role
            if (!context.UserRoles.Any(ur => ur.UserId == user.UserId))
            {
                context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole.Id });
            }

            // 2. Create User Profile
            if (user.Profile == null)
            {
                var profile = new UserProfile
                {
                    UserId = user.UserId,
                    FullName = user.Email.Split('@')[0], 
                    Dob = _faker.Date.Past(40, DateTime.UtcNow.AddYears(-18)),
                    Gender = _faker.PickRandom("Nam", "Nữ", "Khác"),
                    HeightCm = _faker.Random.Decimal(150, 190),
                    WeightKg = _faker.Random.Decimal(45, 100),
                    ActivityLevel = _faker.PickRandom("Sedentary", "Light", "Moderate", "Active", "VeryActive"),
                    AvatarUrl = _faker.Internet.Avatar()
                };
                context.UserProfiles.Add(profile);
            }

            // 3. Create Goals 
            var goal = new Goal
            {
                UserId = user.UserId,
                Type = _faker.PickRandom("WeightLoss", "MuscleGain", "Maintenance"),
                TargetValue = _faker.Random.Decimal(50, 90),
                StartDate = user.CreatedAt,
                EndDate = user.CreatedAt.AddMonths(6),
                Status = "InProgress",
                Notes = "Mục tiêu tập luyện năm nay"
            };
            context.Goals.Add(goal);

            // 4. Generate Workout Logs (Last 3 months)
            SeedWorkoutsForUser(context, user.UserId, exerciseIds);

            // 5. Generate Nutrition Logs (Last 3 months)
            SeedNutritionForUser(context, user.UserId, foodItemIds, foodDict);
        }
        
        context.SaveChanges();
    }

    private static void SeedWorkoutsForUser(HealthSyncDbContext context, int userId, List<int> exerciseIds)
    {
        int workoutCount = _faker.Random.Int(20, 40);
        for (int w = 0; w < workoutCount; w++)
        {
            var workoutDate = _faker.Date.Recent(90);
            var workoutLog = new WorkoutLog
            {
                UserId = userId,
                WorkoutDate = workoutDate,
                DurationMin = _faker.Random.Int(30, 90),
                Notes = _faker.Lorem.Sentence(),
            };
            
            int sessionCount = _faker.Random.Int(3, 6);
            for (int s = 0; s < sessionCount; s++)
            {
                var exerciseId = _faker.PickRandom(exerciseIds);
                workoutLog.ExerciseSessions.Add(new ExerciseSession
                {
                    ExerciseId = exerciseId,
                    Sets = _faker.Random.Int(3, 5),
                    Reps = _faker.Random.Int(8, 15),
                    WeightKg = _faker.Random.Decimal(10, 80),
                    Rpe = _faker.Random.Decimal(6, 9)
                });
            }
            context.WorkoutLogs.Add(workoutLog);
        }
    }

    private static void SeedNutritionForUser(HealthSyncDbContext context, int userId, List<int> foodIds, Dictionary<int, FoodItem> foodDict)
    {
         int nutritionCount = _faker.Random.Int(40, 60);
         for (int n = 0; n < nutritionCount; n++)
         {
             var logDate = _faker.Date.Recent(90);
             var nutritionLog = new NutritionLog
             {
                 UserId = userId,
                 LogDate = logDate,
                 Notes = _faker.PickRandom("Ngon", "Tạm được", null)
             };

             int entryCount = _faker.Random.Int(2, 5);
             decimal tCal = 0, tP = 0, tC = 0, tF = 0;

             for (int e = 0; e < entryCount; e++)
             {
                 var fId = _faker.PickRandom(foodIds);
                 var food = foodDict[fId];
                 var qty = _faker.Random.Decimal(0.5m, 2.0m);
                 
                 var entry = new FoodEntry
                 {
                     FoodItemId = fId,
                     Quantity = qty * food.ServingSize, 
                     CaloriesKcal = food.CaloriesKcal * qty,
                     ProteinG = food.ProteinG * qty,
                     CarbsG = food.CarbsG * qty,
                     FatG = food.FatG * qty
                 };
                 
                 tCal += entry.CaloriesKcal ?? 0;
                 tP += entry.ProteinG ?? 0;
                 tC += entry.CarbsG ?? 0;
                 tF += entry.FatG ?? 0;

                 nutritionLog.FoodEntries.Add(entry);
             }

             nutritionLog.TotalCalories = tCal;
             nutritionLog.ProteinG = tP;
             nutritionLog.CarbsG = tC;
             nutritionLog.FatG = tF;

             context.NutritionLogs.Add(nutritionLog);
         }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
