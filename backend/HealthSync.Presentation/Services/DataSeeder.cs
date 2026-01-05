using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using Bogus;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using HealthSync.Domain.Constants;

namespace HealthSync.Presentation.Services;

public class DataSeeder
{
    private readonly IMinioClient _minioClient;
    private readonly HealthSyncDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    private readonly AvatarSeeder _avatarSeeder;
    private const string BUCKET_NAME = "healthsync-files";
    // private const string DEFAULT_MINIO_URL = "http://localhost:9002"; // Removed hardcoded URI

    public DataSeeder(IMinioClient minioClient, HealthSyncDbContext dbContext, IConfiguration configuration, IWebHostEnvironment env, HttpClient httpClient)
    {
        _minioClient = minioClient;
        _dbContext = dbContext;
        _configuration = configuration;
        _env = env;
        var publicUrl = configuration["MinIO:PublicUrl"] ?? throw new InvalidOperationException("MinIO:PublicUrl is not configured");
        _avatarSeeder = new AvatarSeeder(minioClient, httpClient, publicUrl);
    }

    public async Task SeedAsync()
    {
        // Sử dụng distributed lock để tránh 2 backend instances cùng seed
        using var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "sp_getapplock";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        
        var lockNameParam = command.CreateParameter();
        lockNameParam.ParameterName = "@Resource";
        lockNameParam.Value = "HealthSync_DataSeeder";
        command.Parameters.Add(lockNameParam);
        
        var lockModeParam = command.CreateParameter();
        lockModeParam.ParameterName = "@LockMode";
        lockModeParam.Value = "Exclusive";
        command.Parameters.Add(lockModeParam);
        
        var lockOwnerParam = command.CreateParameter();
        lockOwnerParam.ParameterName = "@LockOwner";
        lockOwnerParam.Value = "Session";
        command.Parameters.Add(lockOwnerParam);
        
        var lockTimeoutParam = command.CreateParameter();
        lockTimeoutParam.ParameterName = "@LockTimeout";
        lockTimeoutParam.Value = 5000; // 5 seconds timeout
        command.Parameters.Add(lockTimeoutParam);
        
        var returnParam = command.CreateParameter();
        returnParam.ParameterName = "@ReturnValue";
        returnParam.Direction = System.Data.ParameterDirection.ReturnValue;
        command.Parameters.Add(returnParam);

        try
        {
            await command.ExecuteNonQueryAsync();
            var lockResult = returnParam.Value != null ? (int)returnParam.Value : -1;
            
            if (lockResult < 0)
            {
                Console.WriteLine($"[Info] Another backend instance is seeding data. Skipping... (Lock result: {lockResult})");
                return;
            }
            
            Console.WriteLine("[Info] Acquired seeding lock. Starting data seeding...");
            
            // Thực hiện seeding
            await SeedBucketAsync();
            await SeedImagesAsync();
            await SeedExercisesAsync();
            await SeedFoodItemsAsync();
            await SeedFakeUsersAsync();
            await FixExistingUsersAvatars(); // Fix avatars for existing users
            
            // ===> THÊM MỚI: Seed Action Logs cho Data Warehouse <===
            await SeedUserActionsAsync();
            
            Console.WriteLine("[Success] Data seeding completed.");
        }
        finally
        {
            // Release lock
            using var releaseLockCommand = connection.CreateCommand();
            releaseLockCommand.CommandText = "sp_releaseapplock";
            releaseLockCommand.CommandType = System.Data.CommandType.StoredProcedure;
            
            var releaseParam = releaseLockCommand.CreateParameter();
            releaseParam.ParameterName = "@Resource";
            releaseParam.Value = "HealthSync_DataSeeder";
            releaseLockCommand.Parameters.Add(releaseParam);
            
            var releaseOwnerParam = releaseLockCommand.CreateParameter();
            releaseOwnerParam.ParameterName = "@LockOwner";
            releaseOwnerParam.Value = "Session";
            releaseLockCommand.Parameters.Add(releaseOwnerParam);
            
            await releaseLockCommand.ExecuteNonQueryAsync();
            Console.WriteLine("[Info] Released seeding lock.");
        }
    }

    private async Task SeedBucketAsync()
    {
        // 1. Ensure Bucket exists
        bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BUCKET_NAME));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BUCKET_NAME));

            // Set public policy
            var policyJson = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [{
                    ""Effect"": ""Allow"",
                    ""Principal"": { ""AWS"": [""*""] },
                    ""Action"": [""s3:GetObject""],
                    ""Resource"": [""arn:aws:s3:::" + BUCKET_NAME + @"/*""]
                }]
            }";
            await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(BUCKET_NAME).WithPolicy(policyJson));
        }
    }

    private async Task SeedImagesAsync()
    {
        // 2. Seed Images to MinIO
        var imagePath = Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "Images");
        
        // DEBUG LOG: In ra để xem đường dẫn thực tế đang trỏ đi đâu
        Console.WriteLine($"[Seeder] Looking for images in: {imagePath}");

        if (!Directory.Exists(imagePath))
        {
            Console.WriteLine($"[Error] Directory not found: {imagePath}");
            return;
        }

        var images = new Dictionary<string, string>
        {
            { "bench_press.jpg", "exercises/bench_press.jpg" },
            { "squat.jpg", "exercises/squat.jpg" },
            { "deadlift.jpg", "exercises/deadlift.jpg" },
            { "pull_up.jpg", "exercises/pull_up.jpg" },
            { "push_up.jpg", "exercises/push_up.jpg" },
            { "lunges.jpg", "exercises/lunges.jpg" },
            { "overhead_press.jpg", "exercises/overhead_press.jpg" },
            { "bicep_curl.jpg", "exercises/bicep_curl.jpg" },
            { "tricep_dips.jpg", "exercises/tricep_dips.jpg" },
            { "plank.jpg", "exercises/plank.jpg" },
            { "russian_twist.jpg", "exercises/russian_twist.jpg" },
            { "burpees.jpg", "exercises/burpees.jpg" },
            { "mountain_climbers.jpg", "exercises/mountain_climbers.jpg" },
            { "jumping_jacks.jpg", "exercises/jumping_jacks.jpg" },
            { "calf_raises.jpg", "exercises/calf_raises.jpg" },
            { "apple.png", "foods/apple.png" },
            { "chicken_breast.jpg", "foods/chicken_breast.jpg" },
            { "rice.jpg", "foods/rice.jpg" },
            { "banana.jpg", "foods/banana.jpg" },
            { "salmon.jpg", "foods/salmon.jpg" },
            { "broccoli.jpg", "foods/broccoli.jpg" },
            { "egg.jpg", "foods/egg.jpg" },
            { "oatmeal.jpg", "foods/oatmeal.jpg" },
            { "spinach.jpg", "foods/spinach.jpg" },
            { "avocado.jpg", "foods/avocado.jpg" },
            { "sweet_potato.jpg", "foods/sweet_potato.jpg" },
            { "greek_yogurt.jpg", "foods/greek_yogurt.jpg" },
            { "almonds.jpg", "foods/almonds.jpg" },
            { "tomato.jpg", "foods/tomato.jpg" },
            { "quinoa.jpg", "foods/quinoa.jpg" }
        };

        foreach (var (fileName, objectName) in images)
        {
            var filePath = Path.Combine(imagePath, fileName);
            
            if (!File.Exists(filePath))
            {
                // Báo lỗi nếu thiếu file nguồn
                Console.WriteLine($"[Warning] Source file missing: {fileName}");
                continue;
            }

            bool objectExists = false;
            try
            {
                await _minioClient.StatObjectAsync(new StatObjectArgs().WithBucket(BUCKET_NAME).WithObject(objectName));
                objectExists = true;
                Console.WriteLine($"[Info] File already exists in MinIO: {objectName}");
            }
            catch (MinioException)
            {
                // Chỉ coi là chưa tồn tại nếu lỗi là ObjectNotFound
                objectExists = false;
            }

            if (!objectExists)
            {
                try
                {
                    using var fileStream = File.OpenRead(filePath);
                    var contentType = fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                      fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                      ? "image/jpeg" : "image/png";

                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(BUCKET_NAME)
                        .WithObject(objectName)
                        .WithStreamData(fileStream)
                        .WithObjectSize(fileStream.Length)
                        .WithContentType(contentType));
                    
                    Console.WriteLine($"[Success] Uploaded: {objectName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to upload {objectName}: {ex.Message}");
                }
            }
        }
    }

    private async Task SeedExercisesAsync()
    {
        // 3. Seed Exercises - Clear old data and reseed from scratch
        var exercisesJson = await File.ReadAllTextAsync(Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "exercises.json"));
        
        // FIX: Thêm options để đọc JSON không phân biệt hoa thường
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var exercisesData = JsonSerializer.Deserialize<List<ExerciseSeedData>>(exercisesJson, options);
        
        if (exercisesData != null)
        {
            var publicUrl = _configuration["MinIO:PublicUrl"] ?? throw new InvalidOperationException("MinIO:PublicUrl is not configured");
            
            // CHECK: Nếu đã có dữ liệu thì KHÔNG seed lại (giữ dữ liệu cũ)
            if (await _dbContext.Exercises.AnyAsync())
            {
                Console.WriteLine("[Info] Exercises data already exists. Skipping seed.");
                return;
            }
            
            // SEED MỚI (chỉ chạy khi chưa có data)
            var exercises = new List<Exercise>();
            
            foreach (var e in exercisesData)
            {
                // Tạo tên file và URL theo quy tắc
                var imageName = $"{e.Name.ToLower().Replace(" ", "_").Replace("-", "_")}.jpg";
                var objectName = $"exercises/{imageName}";
                var imageUrl = $"{publicUrl}/{BUCKET_NAME}/{objectName}";
                
                exercises.Add(new Exercise
                {
                    Name = e.Name,
                    MuscleGroup = e.MuscleGroup,
                    Difficulty = e.Difficulty,
                    Equipment = e.Equipment,
                    Description = e.Description,
                    ImageUrl = imageUrl // Luôn có URL
                });
                
                Console.WriteLine($"[Info] Prepared exercise: {e.Name} with URL: {imageUrl}");
            }
            
            _dbContext.Exercises.AddRange(exercises);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[Success] Seeded {exercises.Count} exercises with images.");
        }
    }

    private async Task SeedFoodItemsAsync()
    {
        // 4. Seed FoodItems - Check existing
        var foodsJson = await File.ReadAllTextAsync(Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "foods.json"));
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var foodsData = JsonSerializer.Deserialize<List<FoodItemSeedData>>(foodsJson, options);
        
        if (foodsData != null)
        {
            var publicUrl = _configuration["MinIO:PublicUrl"] ?? throw new InvalidOperationException("MinIO:PublicUrl is not configured");
            
            // CHECK: Nếu đã có dữ liệu thì KHÔNG seed lại
            if (await _dbContext.FoodItems.AnyAsync())
            {
                Console.WriteLine("[Info] Food items data already exists. Skipping seed.");
                return;
            }
            
            // SEED MỚI
            var foodItems = new List<FoodItem>();
            
            foreach (var f in foodsData)
            {
                // Tạo tên file và URL theo quy tắc
                var extension = f.Name == "Apple" ? ".png" : ".jpg";
                var imageName = $"{f.Name.ToLower().Replace(" ", "_")}{extension}";
                var objectName = $"foods/{imageName}";
                var imageUrl = $"{publicUrl}/{BUCKET_NAME}/{objectName}";
                
                foodItems.Add(new FoodItem
                {
                    Name = f.Name,
                    ServingSize = f.ServingSize,
                    ServingUnit = f.ServingUnit,
                    CaloriesKcal = f.Calories,
                    ProteinG = f.Protein,
                    CarbsG = f.Carbs,
                    FatG = f.Fat,
                    ImageUrl = imageUrl // Luôn có URL
                });
                
                Console.WriteLine($"[Info] Prepared food: {f.Name} with URL: {imageUrl}");
            }
            
            _dbContext.FoodItems.AddRange(foodItems);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[Success] Seeded {foodItems.Count} food items with images.");
        }
    }

    private async Task SeedFakeUsersAsync()
    {
        // 5. Seed Fake Users with Rich Transactional Data
        await EnsureUsersAsync();
        await EnsureUserActivityAsync();
    }

    private async Task EnsureUsersAsync()
    {
        int currentCount = await _dbContext.ApplicationUsers.CountAsync();
        if (currentCount < 50)
        {
            int limit = 50 - currentCount;
            Console.WriteLine($"[Seeder] Generating {limit} fake users...");
            
            // Preload sample avatars
            var sampleAvatars = await _avatarSeeder.SeedSampleAvatarsAsync();
            var random = new Random();

            // Ensure Customer role exists
            var customerRole = await EnsureCustomerRoleAsync();

            // Configure faker for UserProfile
            var profileFaker = new Faker<UserProfile>("vi") // Vietnamese locale
                .RuleFor(p => p.FullName, f => f.Name.FullName())
                .RuleFor(p => p.Gender, f => f.PickRandom("Male", "Female"))
                .RuleFor(p => p.Dob, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                .RuleFor(p => p.HeightCm, f => f.Random.Decimal(150, 190))
                .RuleFor(p => p.WeightKg, f => f.Random.Decimal(45, 90))
                .RuleFor(p => p.ActivityLevel, f => f.PickRandom("Sedentary", "Light", "Moderate", "Active", "VeryActive"));

            // Configure faker for ApplicationUser
            string passwordHash = HashPassword("Password123!");
            var userFaker = new Faker<ApplicationUser>("vi")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.UserName, (f, u) => u.Email)
                .RuleFor(u => u.PasswordHash, f => passwordHash)
                .RuleFor(u => u.IsActive, f => true)
                .RuleFor(u => u.EmailConfirmed, f => true)
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));

            // Generate users
            var fakeUsers = userFaker.Generate(limit);
            
            int processedCount = 0;
            foreach (var user in fakeUsers)
            {
                // Random gán avatar từ sample
                var randomAvatar = sampleAvatars[random.Next(sampleAvatars.Count)];
                user.AvatarUrl = randomAvatar;

                // Add user first to get UserId
                _dbContext.ApplicationUsers.Add(user);
                await _dbContext.SaveChangesAsync();

                // Create corresponding profile
                var profile = profileFaker.Generate();
                profile.UserId = user.UserId;
                _dbContext.UserProfiles.Add(profile);

                // Create UserRole
                _dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole.Id });
                
                // Create 3 Goals: NotStarted, InProgress, Completed
                Create3Goals(user.UserId, user.CreatedAt, profile.WeightKg);

                processedCount++;
                if (processedCount % 10 == 0)
                {
                    Console.WriteLine($"[Seeder] Created {processedCount}/{limit} users...");
                }
            }
            
            await _dbContext.SaveChangesAsync(); // Final save
            Console.WriteLine($"[Success] Generated {limit} fake users.");
        }
    }

    private async Task EnsureUserActivityAsync()
    {
        Console.WriteLine("[Seeder] Checking and generating user activity...");
        
        var exerciseIds = await _dbContext.Exercises.Select(e => e.ExerciseId).ToListAsync();
        var foodItems = await _dbContext.FoodItems.ToListAsync();
        
        if (!exerciseIds.Any() || !foodItems.Any())
        {
            Console.WriteLine("[Warning] No exercises or mock foods found. Skipping activity generation.");
            return;
        }

        var oneDayAgo = DateTime.UtcNow.AddDays(-1);
        var users = await _dbContext.ApplicationUsers
            .Include(u => u.WorkoutLogs).ThenInclude(w => w.ExerciseSessions)
            .Include(u => u.NutritionLogs).ThenInclude(n => n.FoodEntries)
            .Where(u => u.CreatedAt < oneDayAgo)
            .Take(100)
            .ToListAsync();
            
        Console.WriteLine($"[Seeder] Found {users.Count} existing users to seed activity.");
        
        var random = new Random();
        int updatedUsers = 0;

        foreach (var user in users)
        {
            // Evaluate both methods separately to ensure both are always called
            bool workoutUpdated = ProcessWorkoutLogs(user, random, exerciseIds);
            bool nutritionUpdated = ProcessNutritionLogs(user, random, foodItems);
            bool hasUpdates = workoutUpdated || nutritionUpdated;

            if (hasUpdates)
            {
                updatedUsers++;
                if (updatedUsers % 10 == 0) Console.WriteLine($"[Seeder] Updated activity for {updatedUsers} users...");
            }
        }

        if (updatedUsers > 0)
        {
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[Success] Generated activity for {updatedUsers} existing users.");
        }
        else
        {
            Console.WriteLine($"[Info] All existing users already have activity.");
        }
    }

    private bool ProcessWorkoutLogs(ApplicationUser user, Random random, List<int> exerciseIds)
    {
        if (!user.WorkoutLogs.Any())
        {
            CreateRecentWorkoutLogs(user.UserId, random, exerciseIds);
            return true;
        }

        bool hasUpdates = false;
        foreach (var log in user.WorkoutLogs)
        {
            if (!log.ExerciseSessions.Any() && exerciseIds.Any())
            {
                BackfillExerciseSessions(log, random, exerciseIds);
                hasUpdates = true;
            }
        }
        return hasUpdates;
    }

    private void BackfillExerciseSessions(WorkoutLog log, Random random, List<int> exerciseIds)
    {
        int sessionCount = random.Next(4, 8);
        for (int s = 0; s < sessionCount; s++)
        {
            log.ExerciseSessions.Add(new ExerciseSession
            {
                ExerciseId = exerciseIds[random.Next(exerciseIds.Count)],
                Sets = random.Next(3, 6),
                Reps = random.Next(8, 15),
                WeightKg = (decimal)random.Next(10, 100),
                Rpe = (decimal)random.NextDouble() * 3 + 6
            });
        }
    }

    private bool ProcessNutritionLogs(ApplicationUser user, Random random, List<FoodItem> foodItems)
    {
        if (!user.NutritionLogs.Any())
        {
            CreateRecentNutritionLogs(user.UserId, random, foodItems);
            return true;
        }

        bool hasUpdates = false;
        foreach (var log in user.NutritionLogs)
        {
            if (!log.FoodEntries.Any() && foodItems.Any())
            {
                BackfillFoodEntries(log, random, foodItems);
                hasUpdates = true;
            }
        }
        return hasUpdates;
    }

    private void BackfillFoodEntries(NutritionLog log, Random random, List<FoodItem> foodItems)
    {
        int entryCount = random.Next(3, 6);
        decimal tCal = 0, tP = 0, tC = 0, tF = 0;
        var mealTypes = new[] { "Breakfast", "Lunch", "Dinner", "Snack" };

        for (int e = 0; e < entryCount; e++)
        {
            var food = foodItems[random.Next(foodItems.Count)];
            var qtyRatio = (decimal)(random.NextDouble() * 1.5 + 0.5);

            var entry = new FoodEntry
            {
                FoodItemId = food.FoodItemId,
                Quantity = qtyRatio * food.ServingSize,
                CaloriesKcal = food.CaloriesKcal * qtyRatio,
                ProteinG = food.ProteinG * qtyRatio,
                CarbsG = food.CarbsG * qtyRatio,
                FatG = food.FatG * qtyRatio,
                MealType = mealTypes[random.Next(mealTypes.Length)]
            };

            log.FoodEntries.Add(entry);
            tCal += entry.CaloriesKcal ?? 0;
            tP += entry.ProteinG ?? 0;
            tC += entry.CarbsG ?? 0;
            tF += entry.FatG ?? 0;
        }

        log.TotalCalories = tCal;
        log.ProteinG = tP;
        log.CarbsG = tC;
        log.FatG = tF;
    }

    private async Task<Role> EnsureCustomerRoleAsync()
    {
        var customerRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
        if (customerRole == null)
        {
            customerRole = new Role
            {
                RoleName = "Customer",
                Description = "Regular customer user"
            };
            _dbContext.Roles.Add(customerRole);
            await _dbContext.SaveChangesAsync();
        }
        return customerRole;
    }

    private void Create3Goals(int userId, DateTime createdAt, decimal? weightKg)
    {
        var faker = new Faker();
        var random = new Random();
        var currentWeight = weightKg ?? 70;
        
        // Goal 1: NotStarted - Future goal for muscle gain
        var goal1 = new Goal
        {
            UserId = userId,
            Type = "muscle_gain",
            TargetValue = currentWeight + (decimal)5.0, // Target: Gain 5kg (increase goal)
            StartDate = DateTime.UtcNow.AddDays(7), // Starts next week
            EndDate = DateTime.UtcNow.AddMonths(4),
            Status = "not_started",
            Notes = "Mục tiêu tăng cơ - bắt đầu tuần sau"
        };
        _dbContext.Goals.Add(goal1);
        // NotStarted goal: No progress records yet
        
        // Goal 2: InProgress - Current weight loss goal
        // For weight_loss: startValue > currentValue > targetValue (decreasing)
        var goal2StartWeight = currentWeight + (decimal)7.0; // Started 7kg heavier
        var goal2TargetWeight = currentWeight - (decimal)3.0; // Target: lose 10kg total (7+3)
        var goal2 = new Goal
        {
            UserId = userId,
            Type = "weight_loss",
            TargetValue = goal2TargetWeight, // Target value < start value (decrease goal)
            StartDate = createdAt.AddDays(-30), // Started 30 days ago
            EndDate = createdAt.AddMonths(5),
            Status = "in_progress",
            Notes = "Mục tiêu giảm cân - đang tiến hành tốt"
        };
        _dbContext.Goals.Add(goal2);
        
        // Add progress records for InProgress goal (showing 70% progress: lost 7kg out of 10kg)
        var goal2TotalChange = goal2StartWeight - goal2TargetWeight; // 10kg to lose
        var goal2ProgressCount = random.Next(4, 7); // 4-6 progress records
        
        // First record: starting weight
        goal2.ProgressRecords.Add(new ProgressRecord
        {
            RecordDate = createdAt.AddDays(-30),
            Value = goal2StartWeight,
            WeightKg = goal2StartWeight,
            WaistCm = 85,
            Notes = "Bắt đầu hành trình giảm cân"
        });
        
        // Progressive weight loss records
        for (int i = 1; i <= goal2ProgressCount; i++)
        {
            var daysFromStart = (30.0 / (goal2ProgressCount + 1)) * (i + 1);
            var recordDate = createdAt.AddDays(-30 + daysFromStart);
            // Progress from 20% to 70% (already lost 7kg out of 10kg)
            var progressRatio = 0.2m + (0.5m * i / goal2ProgressCount);
            var weightLost = goal2TotalChange * progressRatio;
            var currentValue = goal2StartWeight - weightLost;
            
            goal2.ProgressRecords.Add(new ProgressRecord
            {
                RecordDate = recordDate,
                Value = currentValue,
                WeightKg = currentValue,
                WaistCm = 85 - (decimal)(progressRatio * 5), // Waist decreases proportionally
                Notes = i == goal2ProgressCount ? "Tiến độ tốt, còn 3kg nữa!" : null
            });
        }
        
        // Goal 3: Completed - Past weight gain goal (opposite direction)
        var goal3StartWeight = currentWeight - (decimal)4.0; // Started 4kg lighter
        var goal3TargetWeight = currentWeight; // Target: gain to current weight
        var goal3 = new Goal
        {
            UserId = userId,
            Type = "weight_gain",
            TargetValue = goal3TargetWeight, // Target value > start value (increase goal)
            StartDate = createdAt.AddMonths(-6),
            EndDate = createdAt.AddDays(-5), // Ended 5 days ago
            Status = "completed",
            Notes = "Mục tiêu tăng cân lành mạnh - hoàn thành!"
        };
        _dbContext.Goals.Add(goal3);
        
        // Add progress records for Completed goal (showing successful weight gain from start to target)
        var goal3ProgressCount = random.Next(6, 10); // 6-9 progress records over 6 months
        var goal3Duration = 180.0; // ~6 months in days
        var goal3TotalChange = goal3TargetWeight - goal3StartWeight; // 4kg to gain
        
        // First record: starting weight
        goal3.ProgressRecords.Add(new ProgressRecord
        {
            RecordDate = createdAt.AddMonths(-6),
            Value = goal3StartWeight,
            WeightKg = goal3StartWeight,
            WaistCm = 76,
            Notes = "Bắt đầu tăng cân"
        });
        
        // Progressive weight gain records
        for (int i = 1; i <= goal3ProgressCount; i++)
        {
            var daysFromStart = (goal3Duration / (goal3ProgressCount + 1)) * (i + 1);
            var recordDate = createdAt.AddMonths(-6).AddDays(daysFromStart);
            // Progress from 10% to 100% with slight fluctuation
            var baseProgressRatio = (decimal)i / goal3ProgressCount;
            var fluctuation = (decimal)(random.NextDouble() * 0.1 - 0.05); // ±5% fluctuation
            var progressRatio = Math.Min(1.0m, Math.Max(0.1m, baseProgressRatio + fluctuation));
            var weightGained = goal3TotalChange * progressRatio;
            var currentValue = goal3StartWeight + weightGained;
            
            goal3.ProgressRecords.Add(new ProgressRecord
            {
                RecordDate = recordDate,
                Value = currentValue,
                WeightKg = currentValue,
                WaistCm = 76 + (decimal)(progressRatio * 4), // Waist increases slightly
                Notes = i == goal3ProgressCount ? "Hoàn thành mục tiêu tăng cân!" : null
            });
        }
    }

    private void CreateRecentWorkoutLogs(int userId, Random random, List<int> exerciseIds)
    {
        var existingWorkoutDates = _dbContext.WorkoutLogs
            .Where(w => w.UserId == userId)
            .Select(w => w.WorkoutDate.Date)
            .ToHashSet();
        
        int workoutCount = random.Next(30, 51);
        var now = DateTime.UtcNow;
        var createdDates = new HashSet<DateTime>();
        
        for (int w = 0; w < workoutCount; w++)
        {
            var workoutDate = GenerateRandomDate(random, now, existingWorkoutDates, createdDates, 10);
            if (workoutDate == null) continue;
            
            createdDates.Add(workoutDate.Value.Date);
            
            var workoutLog = new WorkoutLog
            {
                UserId = userId,
                WorkoutDate = workoutDate.Value,
                DurationMin = random.Next(30, 120),
                Notes = new Faker().Lorem.Sentence(),
            };
            
            AddExerciseSessions(workoutLog, random, exerciseIds);
            _dbContext.WorkoutLogs.Add(workoutLog);
        }
    }

    private void AddExerciseSessions(WorkoutLog workoutLog, Random random, List<int> exerciseIds)
    {
        if (!exerciseIds.Any()) return;
        
        int sessionCount = random.Next(4, 8);
        for (int s = 0; s < sessionCount; s++)
        {
            workoutLog.ExerciseSessions.Add(new ExerciseSession
            {
                ExerciseId = exerciseIds[random.Next(exerciseIds.Count)],
                Sets = random.Next(3, 6),
                Reps = random.Next(8, 15),
                WeightKg = (decimal)random.Next(10, 100),
                Rpe = (decimal)random.NextDouble() * 3 + 6
            });
        }
    }

    private void CreateRecentNutritionLogs(int userId, Random random, List<FoodItem> foodItems)
    {
        var existingLogDates = _dbContext.NutritionLogs
            .Where(n => n.UserId == userId)
            .Select(n => n.LogDate.Date)
            .ToHashSet();
        
        int nutritionCount = random.Next(60, 91);
        var now = DateTime.UtcNow;
        var createdDates = new HashSet<DateTime>();
        
        for (int n = 0; n < nutritionCount; n++)
        {
            var logDate = GenerateRandomDate(random, now, existingLogDates, createdDates, 10);
            if (logDate == null) continue;
            
            createdDates.Add(logDate.Value.Date);
            
            var nutritionLog = new NutritionLog
            {
                UserId = userId,
                LogDate = logDate.Value,
                Notes = new Faker().PickRandom("Ăn ngon", "Hơi no", "Healthy meal", "Bữa chính", null)
            };

            if (foodItems.Any())
            {
                var (tCal, tP, tC, tF) = AddMealEntries(nutritionLog, random, foodItems);
                
                if (random.NextDouble() < 0.5)
                {
                    var snackTotals = AddSnackEntries(nutritionLog, random, foodItems);
                    tCal += snackTotals.cal;
                    tP += snackTotals.p;
                    tC += snackTotals.c;
                    tF += snackTotals.f;
                }
                
                nutritionLog.TotalCalories = tCal;
                nutritionLog.ProteinG = tP;
                nutritionLog.CarbsG = tC;
                nutritionLog.FatG = tF;
            }
            _dbContext.NutritionLogs.Add(nutritionLog);
        }
    }

    private DateTime? GenerateRandomDate(Random random, DateTime now, HashSet<DateTime> existingDates, HashSet<DateTime> createdDates, int maxAttempts)
    {
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            var rand = random.NextDouble();
            DateTime date = rand < 0.30 
                ? now.AddDays(-random.Next(0, 2)).AddHours(-random.Next(0, 24))
                : rand < 0.70 
                    ? now.AddDays(-random.Next(2, 8)).AddHours(-random.Next(0, 24))
                    : now.AddDays(-random.Next(8, 91)).AddHours(-random.Next(0, 24));

            if (!existingDates.Contains(date.Date) && !createdDates.Contains(date.Date))
                return date;
        }
        return null;
    }

    private (decimal cal, decimal p, decimal c, decimal f) AddMealEntries(NutritionLog log, Random random, List<FoodItem> foodItems)
    {
        decimal tCal = 0, tP = 0, tC = 0, tF = 0;
        var requiredMeals = new[] { "Breakfast", "Lunch", "Dinner" };
        
        foreach (var mealType in requiredMeals)
        {
            int itemsInMeal = random.Next(1, 3);
            for (int i = 0; i < itemsInMeal; i++)
            {
                var totals = AddFoodEntry(log, random, foodItems, mealType, 0.5, 2.0);
                tCal += totals.cal; tP += totals.p; tC += totals.c; tF += totals.f;
            }
        }
        return (tCal, tP, tC, tF);
    }

    private (decimal cal, decimal p, decimal c, decimal f) AddSnackEntries(NutritionLog log, Random random, List<FoodItem> foodItems)
    {
        decimal tCal = 0, tP = 0, tC = 0, tF = 0;
        int snackCount = random.Next(1, 3);
        
        for (int i = 0; i < snackCount; i++)
        {
            var totals = AddFoodEntry(log, random, foodItems, "Snack", 0.3, 1.3);
            tCal += totals.cal; tP += totals.p; tC += totals.c; tF += totals.f;
        }
        return (tCal, tP, tC, tF);
    }

    private (decimal cal, decimal p, decimal c, decimal f) AddFoodEntry(NutritionLog log, Random random, List<FoodItem> foodItems, string mealType, double minRatio, double maxRatio)
    {
        var food = foodItems[random.Next(foodItems.Count)];
        var qtyRatio = (decimal)(random.NextDouble() * (maxRatio - minRatio) + minRatio);

        var entry = new FoodEntry
        {
            FoodItemId = food.FoodItemId,
            Quantity = qtyRatio * food.ServingSize,
            CaloriesKcal = food.CaloriesKcal * qtyRatio,
            ProteinG = food.ProteinG * qtyRatio,
            CarbsG = food.CarbsG * qtyRatio,
            FatG = food.FatG * qtyRatio,
            MealType = mealType
        };

        log.FoodEntries.Add(entry);
        return (entry.CaloriesKcal ?? 0, entry.ProteinG ?? 0, entry.CarbsG ?? 0, entry.FatG ?? 0);
    }
    
    // Keep old methods for backward compatibility but unused now
    private void CreateFakeWorkoutLogs(int userId, Random random, List<int> exerciseIds)
    {
        // Get existing workout dates to avoid duplicates
        var existingWorkoutDates = _dbContext.WorkoutLogs
            .Where(w => w.UserId == userId)
            .Select(w => w.WorkoutDate.Date)
            .ToHashSet();
        
        int workoutCount = random.Next(15, 30);
        var createdDates = new HashSet<DateTime>();
        
        for (int w = 0; w < workoutCount; w++)
        {
            DateTime workoutDate;
            int attempts = 0;
            
            // Try to find a unique date (max 10 attempts per workout)
            do
            {
                workoutDate = new Faker().Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
                attempts++;
            } while ((existingWorkoutDates.Contains(workoutDate.Date) || createdDates.Contains(workoutDate.Date)) && attempts < 10);
            
            // Skip if we couldn't find a unique date after 10 attempts
            if (existingWorkoutDates.Contains(workoutDate.Date) || createdDates.Contains(workoutDate.Date))
            {
                continue;
            }
            
            createdDates.Add(workoutDate.Date);
            var workoutLog = new WorkoutLog
            {
                UserId = userId,
                WorkoutDate = workoutDate.Date, // Store date only without time
                DurationMin = random.Next(30, 90),
                Notes = new Faker().Lorem.Sentence(),
            };
            
            if (exerciseIds.Any())
            {
                int sessionCount = random.Next(3, 6);
                for (int s = 0; s < sessionCount; s++)
                {
                    var exerciseId = exerciseIds[random.Next(exerciseIds.Count)];
                    workoutLog.ExerciseSessions.Add(new ExerciseSession
                    {
                        ExerciseId = exerciseId,
                        Sets = random.Next(3, 5),
                        Reps = random.Next(8, 15),
                        WeightKg = (decimal)random.Next(10, 80),
                        Rpe = (decimal)random.NextDouble() * 3 + 6 // 6.0 - 9.0
                    });
                }
            }
            _dbContext.WorkoutLogs.Add(workoutLog);
        }
    }

    private void CreateFakeNutritionLogs(int userId, Random random, List<FoodItem> foodItems)
    {
        var existingLogDates = _dbContext.NutritionLogs
            .Where(n => n.UserId == userId)
            .Select(n => n.LogDate.Date)
            .ToHashSet();
        
        int nutritionCount = random.Next(30, 50);
        var createdDates = new HashSet<DateTime>();
        var faker = new Faker();
        
        for (int n = 0; n < nutritionCount; n++)
        {
            var logDate = GenerateFakeDate(faker, existingLogDates, createdDates, 10);
            if (logDate == null) continue;
            
            createdDates.Add(logDate.Value.Date);
            
            var nutritionLog = new NutritionLog
            {
                UserId = userId,
                LogDate = logDate.Value.Date,
                Notes = faker.PickRandom("Ăn ngon", "Hơi no", "Healthy meal", null)
            };

            if (foodItems.Any())
            {
                var (tCal, tP, tC, tF) = AddMealEntries(nutritionLog, random, foodItems);
                
                if (random.NextDouble() < 0.5)
                {
                    var snackTotals = AddSnackEntries(nutritionLog, random, foodItems);
                    tCal += snackTotals.cal;
                    tP += snackTotals.p;
                    tC += snackTotals.c;
                    tF += snackTotals.f;
                }
                
                nutritionLog.TotalCalories = tCal;
                nutritionLog.ProteinG = tP;
                nutritionLog.CarbsG = tC;
                nutritionLog.FatG = tF;
            }
            _dbContext.NutritionLogs.Add(nutritionLog);
        }
    }

    private DateTime? GenerateFakeDate(Faker faker, HashSet<DateTime> existingDates, HashSet<DateTime> createdDates, int maxAttempts)
    {
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            var date = faker.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
            if (!existingDates.Contains(date.Date) && !createdDates.Contains(date.Date))
                return date;
        }
        return null;
    }

    private async Task FixExistingUsersAvatars()
    {
        // Fix avatars for all existing users who don't have one
        var usersWithoutAvatar = await _dbContext.ApplicationUsers
            .Where(u => u.AvatarUrl == null || u.AvatarUrl == "")
            .ToListAsync();

        if (!usersWithoutAvatar.Any())
        {
            Console.WriteLine("[Info] All users already have avatars.");
            return;
        }

        Console.WriteLine($"[Seeder] Fixing avatars for {usersWithoutAvatar.Count} users...");

        // Get sample avatars
        var sampleAvatars = await _avatarSeeder.SeedSampleAvatarsAsync();
        var random = new Random();

        foreach (var user in usersWithoutAvatar)
        {
            // Assign random avatar from sample
            user.AvatarUrl = sampleAvatars[random.Next(sampleAvatars.Count)];
        }

        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"[Success] Fixed avatars for {usersWithoutAvatar.Count} users.");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private async Task SeedUserActionsAsync()
    {
        if (await _dbContext.UserActionLogs.AnyAsync())
        {
            Console.WriteLine("[Info] User action logs already exist. Skipping.");
            return;
        }

        Console.WriteLine("[Seeder] Generating User Action Logs (Data Warehouse Lite)...");

        var users = await _dbContext.ApplicationUsers
            .Include(u => u.WorkoutLogs)
            .Include(u => u.NutritionLogs)
            .Include(u => u.Goals)
            .Take(50)
            .ToListAsync();

        var actionLogs = new List<UserActionLog>();
        var random = new Random();

        foreach (var user in users)
        {
            actionLogs.AddRange(GenerateUserActionLogs(user, random));
        }

        var sortedLogs = actionLogs.OrderBy(l => l.Timestamp).ToList();

        const int batchSize = 1000;
        for (int i = 0; i < sortedLogs.Count; i += batchSize)
        {
            _dbContext.UserActionLogs.AddRange(sortedLogs.Skip(i).Take(batchSize));
            await _dbContext.SaveChangesAsync();
        }

        Console.WriteLine($"[Success] Seeded {sortedLogs.Count} action logs for AI Context.");
    }

    private IEnumerable<UserActionLog> GenerateUserActionLogs(ApplicationUser user, Random random)
    {
        var logs = new List<UserActionLog>();
        
        // 1. Login logs
        for (int i = 0; i < 20; i++)
        {
            logs.Add(new UserActionLog
            {
                UserId = user.UserId,
                ActionType = "UserLogin",
                Description = "Người dùng đăng nhập vào hệ thống",
                Timestamp = DateTime.UtcNow.AddDays(-random.Next(0, 30)).AddHours(random.Next(6, 22))
            });
        }

        // 2. Workout logs
        logs.AddRange(user.WorkoutLogs.Select(w => new UserActionLog
        {
            UserId = user.UserId,
            ActionType = "CreateWorkoutLog",
            Description = $"Đã ghi nhận buổi tập: {w.DurationMin} phút",
            Timestamp = w.WorkoutDate.AddMinutes(w.DurationMin + 5)
        }));

        // 3. Nutrition logs
        logs.AddRange(user.NutritionLogs.Select(n => new UserActionLog
        {
            UserId = user.UserId,
            ActionType = "CreateNutritionLog",
            Description = $"Đã ghi nhật ký ăn uống: {n.TotalCalories:F0} kcal",
            Timestamp = n.LogDate.AddHours(random.Next(8, 20))
        }));

        // 4. Goal logs
        foreach (var goal in user.Goals)
        {
            logs.Add(new UserActionLog
            {
                UserId = user.UserId,
                ActionType = "CreateGoal",
                Description = $"Đặt mục tiêu mới: {goal.Type} - {goal.TargetValue}kg",
                Timestamp = goal.StartDate
            });

            if (goal.Status == "completed")
            {
                logs.Add(new UserActionLog
                {
                    UserId = user.UserId,
                    ActionType = "CompleteGoal",
                    Description = $"Chúc mừng! Đã hoàn thành mục tiêu {goal.Type}",
                    Timestamp = goal.EndDate ?? DateTime.UtcNow
                });
            }
        }

        // 5. AI chat logs
        int chatCount = random.Next(3, 8);
        for (int k = 0; k < chatCount; k++)
        {
            logs.Add(new UserActionLog
            {
                UserId = user.UserId,
                ActionType = "ChatWithAI",
                Description = "Đã tương tác với AI Chatbot để được tư vấn",
                Timestamp = DateTime.UtcNow.AddDays(-random.Next(1, 10))
            });
        }

        return logs;
    }
}

public class AvatarSeeder
{
    private readonly IMinioClient _minioClient;
    private readonly HttpClient _httpClient;
    private readonly string _publicUrl;
    private const string BUCKET_NAME = "avatars";

    public AvatarSeeder(IMinioClient minioClient, HttpClient httpClient, string publicUrl)
    {
        _minioClient = minioClient;
        _httpClient = httpClient;
        _publicUrl = publicUrl;
    }

    public async Task<List<string>> SeedSampleAvatarsAsync()
    {
        // 1. Đảm bảo Bucket tồn tại
        bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BUCKET_NAME));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BUCKET_NAME));
            // Set policy public để frontend truy cập được
            var policyJson = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Principal"": {{ ""AWS"": [""*""] }},
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{BUCKET_NAME}/*""]
                    }}
                ]
            }}";
            await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(BUCKET_NAME).WithPolicy(policyJson));
        }

        // 2. Chuẩn bị 5 mẫu avatar (Chỉ tải nếu chưa có)
        var avatarNames = new List<string>();

        for (int i = 1; i <= 5; i++)
        {
            var fileName = $"sample_avatar_{i}.png";
            avatarNames.Add(fileName);

            // Check xem file đã có trên MinIO chưa (để tránh tải lại mỗi lần restart container)
            try
            {
                await _minioClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject(fileName));
                continue; // Đã có rồi thì bỏ qua, đi tiếp
            }
            catch (MinioException)
            {
                // Chưa có thì tải từ DiceBear và Upload lên MinIO
                var diceBearUrl = $"https://api.dicebear.com/7.x/avataaars/png?seed=seed_{i}";
                var imageStream = await _httpClient.GetStreamAsync(diceBearUrl);

                // Upload lên MinIO
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject(fileName)
                    .WithStreamData(imageStream)
                    .WithObjectSize(-1) // Stream ko biết size, MinIO tự handle
                    .WithContentType("image/png"));
            }
        }

        // Trả về list đường dẫn với public URL
        return avatarNames.Select(name => $"{_publicUrl}/{BUCKET_NAME}/{name}").ToList();
    }

    public async Task<string> SeedAvatarAsync(string username)
    {
        // 1. Đảm bảo Bucket tồn tại
        bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BUCKET_NAME));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BUCKET_NAME));
        }

        // 2. Gọi DiceBear API để lấy ảnh về
        var avatarUrl = $"https://api.dicebear.com/9.x/adventurer/png?seed={username}";
        
        try 
        {
            var imageStream = await _httpClient.GetStreamAsync(avatarUrl);
            var objectName = $"{username.ToLower()}_{Guid.NewGuid()}.png";

            // 3. Upload lên MinIO
            using (var memoryStream = new MemoryStream())
            {
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject(objectName)
                    .WithStreamData(memoryStream)
                    .WithObjectSize(memoryStream.Length)
                    .WithContentType("image/png");

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            // 4. Trả về đường dẫn với public URL để lưu vào DB
            return $"{_publicUrl}/{BUCKET_NAME}/{objectName}"; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi seed avatar cho {username}: {ex.Message}");
            return string.Empty;
        }
    }
}

public class ExerciseSeedData
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Equipment { get; set; } = "None";
    public string Description { get; set; } = string.Empty;
}

public class FoodItemSeedData
{
    public string Name { get; set; } = string.Empty;
    public int ServingSize { get; set; }
    public string ServingUnit { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
}