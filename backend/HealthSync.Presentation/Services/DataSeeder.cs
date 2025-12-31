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

namespace HealthSync.Presentation.Services;

public class DataSeeder
{
    private readonly IMinioClient _minioClient;
    private readonly HealthSyncDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    private readonly AvatarSeeder _avatarSeeder;

    public DataSeeder(IMinioClient minioClient, HealthSyncDbContext dbContext, IConfiguration configuration, IWebHostEnvironment env, HttpClient httpClient)
    {
        _minioClient = minioClient;
        _dbContext = dbContext;
        _configuration = configuration;
        _env = env;
        var publicUrl = configuration["MinIO:PublicUrl"] ?? "http://localhost:9002";
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
        var bucketName = "healthsync-files";
        bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));

            // Set public policy
            var policyJson = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [{
                    ""Effect"": ""Allow"",
                    ""Principal"": { ""AWS"": [""*""] },
                    ""Action"": [""s3:GetObject""],
                    ""Resource"": [""arn:aws:s3:::" + bucketName + @"/*""]
                }]
            }";
            await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policyJson));
        }
    }

    private async Task SeedImagesAsync()
    {
        // 2. Seed Images to MinIO
        var bucketName = "healthsync-files";
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
                await _minioClient.StatObjectAsync(new StatObjectArgs().WithBucket(bucketName).WithObject(objectName));
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
                        .WithBucket(bucketName)
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
            var publicUrl = _configuration["MinIO:PublicUrl"] ?? "http://localhost:9002";
            var bucketName = "healthsync-files";
            
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
                var imageUrl = $"{publicUrl}/{bucketName}/{objectName}";
                
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
            var publicUrl = _configuration["MinIO:PublicUrl"] ?? "http://localhost:9002";
            var bucketName = "healthsync-files";
            
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
                var imageUrl = $"{publicUrl}/{bucketName}/{objectName}";
                
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
        // 5. Seed Fake Users for Testing Pagination
        if (await _dbContext.ApplicationUsers.CountAsync() < 50)
        {
            // Preload sample avatars
            var sampleAvatars = await _avatarSeeder.SeedSampleAvatarsAsync();
            var random = new Random();

            // Ensure Customer role exists
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

            // Configure faker for UserProfile
            var profileFaker = new Faker<UserProfile>("vi")
                .RuleFor(p => p.FullName, f => f.Name.FullName())
                .RuleFor(p => p.Gender, f => f.PickRandom("Male", "Female"))
                .RuleFor(p => p.Dob, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                .RuleFor(p => p.HeightCm, f => f.Random.Decimal(150, 190))
                .RuleFor(p => p.WeightKg, f => f.Random.Decimal(45, 90));

            // Configure faker for ApplicationUser
            var userFaker = new Faker<ApplicationUser>("vi")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.PasswordHash, f => "DUMMY_HASH_FOR_TESTING")
                .RuleFor(u => u.IsActive, f => true);

            // Generate 50 fake users
            var fakeUsers = userFaker.Generate(50);

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
                profile.UserId = user.UserId; // Now UserId is set

                // Create UserRole
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = customerRole.Id
                };

                // Add profile and role
                _dbContext.UserProfiles.Add(profile);
                _dbContext.UserRoles.Add(userRole);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

public class AvatarSeeder
{
    private readonly IMinioClient _minioClient;
    private readonly HttpClient _httpClient;
    private readonly string _publicUrl;
    private const string BUCKET_NAME = "avatars";

    public AvatarSeeder(IMinioClient minioClient, HttpClient httpClient, string publicUrl = "http://localhost:9002")
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