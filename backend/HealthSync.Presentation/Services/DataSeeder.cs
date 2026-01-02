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
            
            // CHECK & XÓA DỮ LIỆU CŨ nếu đã có (để đảm bảo đồng bộ 100% với JSON)
            if (await _dbContext.Exercises.AnyAsync())
            {
                _dbContext.Exercises.RemoveRange(_dbContext.Exercises);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("[Info] Cleared old exercises data for reseeding.");
            }
            
            // SEED LẠI TỪ ĐẦU với dữ liệu từ JSON
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
        // 4. Seed FoodItems - Clear old data and reseed from scratch
        var foodsJson = await File.ReadAllTextAsync(Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "foods.json"));
        
        // FIX: Thêm options để đọc JSON không phân biệt hoa thường
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var foodsData = JsonSerializer.Deserialize<List<FoodItemSeedData>>(foodsJson, options);
        
        if (foodsData != null)
        {
            var publicUrl = _configuration["MinIO:PublicUrl"] ?? throw new InvalidOperationException("MinIO:PublicUrl is not configured");
            
            // CHECK & XÓA DỮ LIỆU CŨ nếu đã có (để đảm bảo đồng bộ 100% với JSON)
            if (await _dbContext.FoodItems.AnyAsync())
            {
                _dbContext.FoodItems.RemoveRange(_dbContext.FoodItems);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("[Info] Cleared old food items data for reseeding.");
            }
            
            // SEED LẠI TỪ ĐẦU với dữ liệu từ JSON
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
        if (await _dbContext.ApplicationUsers.CountAsync() < 50)
        {
            Console.WriteLine("[Seeder] Generating 50 fake users with rich history...");
            
            // Preload sample avatars
            var sampleAvatars = await _avatarSeeder.SeedSampleAvatarsAsync();
            var random = new Random();

            // Ensure Customer role exists
            var customerRole = await EnsureCustomerRoleAsync();

            // Fetch IDs and data for random generation
            var exerciseIds = await _dbContext.Exercises.Select(e => e.ExerciseId).ToListAsync();
            var foodItems = await _dbContext.FoodItems.ToListAsync();

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

            // Generate 50 fake users
            var fakeUsers = userFaker.Generate(50);
            
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

                // --- NEW: Generate Transactional Data ---
                CreateFakeGoal(user.UserId, user.CreatedAt, profile.WeightKg);
                CreateFakeWorkoutLogs(user.UserId, random, exerciseIds);
                CreateFakeNutritionLogs(user.UserId, random, foodItems);

                processedCount++;
                if (processedCount % 10 == 0)
                {
                    Console.WriteLine($"[Seeder] Processed {processedCount}/50 users...");
                    await _dbContext.SaveChangesAsync(); // Save in batches
                }
            }
            
            await _dbContext.SaveChangesAsync(); // Final save
            Console.WriteLine("[Success] Generated 50 fake users with full history.");
        }
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

    private void CreateFakeGoal(int userId, DateTime createdAt, decimal? weightKg)
    {
        var goal = new Goal
        {
            UserId = userId,
            Type = new Faker().PickRandom("WeightLoss", "MuscleGain", "Maintenance"),
            TargetValue = (weightKg ?? 60) * (decimal)0.9,
            StartDate = createdAt,
            EndDate = createdAt.AddMonths(6),
            Status = "InProgress",
            Notes = "Mục tiêu 6 tháng đầu năm"
        };
        _dbContext.Goals.Add(goal);
    }

    private void CreateFakeWorkoutLogs(int userId, Random random, List<int> exerciseIds)
    {
        int workoutCount = random.Next(15, 30);
        for (int w = 0; w < workoutCount; w++)
        {
            var workoutDate = new Faker().Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
            var workoutLog = new WorkoutLog
            {
                UserId = userId,
                WorkoutDate = workoutDate,
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
        int nutritionCount = random.Next(30, 50);
        for (int n = 0; n < nutritionCount; n++)
        {
            var logDate = new Faker().Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
            var nutritionLog = new NutritionLog
            {
                UserId = userId,
                LogDate = logDate,
                Notes = new Faker().PickRandom("Ăn ngon", "Hơi no", "Healthy meal", null)
            };

            if (foodItems.Any())
            {
                int entryCount = random.Next(2, 5);
                decimal tCal = 0, tP = 0, tC = 0, tF = 0;

                for (int e = 0; e < entryCount; e++)
                {
                    var food = foodItems[random.Next(foodItems.Count)];
                    var qtyRatio = (decimal)(random.NextDouble() * 1.5 + 0.5); // 0.5 - 2.0 serving

                    var entry = new FoodEntry
                    {
                        FoodItemId = food.FoodItemId,
                        Quantity = qtyRatio * food.ServingSize,
                        CaloriesKcal = food.CaloriesKcal * qtyRatio,
                        ProteinG = food.ProteinG * qtyRatio,
                        CarbsG = food.CarbsG * qtyRatio,
                        FatG = food.FatG * qtyRatio
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
            }
            _dbContext.NutritionLogs.Add(nutritionLog);
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
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