using System.Text.Json;
using Bogus;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Minio;
using Minio.DataModel.Args;

namespace HealthSync.Presentation.Services;

public class DataSeeder
{
    private readonly IMinioClient _minioClient;
    private readonly HealthSyncDbContext _dbContext;
    private readonly IWebHostEnvironment _env;

    public DataSeeder(IMinioClient minioClient, HealthSyncDbContext dbContext, IWebHostEnvironment env)
    {
        _minioClient = minioClient;
        _dbContext = dbContext;
        _env = env;
    }

    public async Task SeedAsync()
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

        // 2. Seed Images to MinIO
        var imagePath = Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "Images");
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
            if (File.Exists(filePath))
            {
                // Check if object exists (simple check by trying to get)
                try
                {
                    await _minioClient.StatObjectAsync(new StatObjectArgs().WithBucket(bucketName).WithObject(objectName));
                }
                catch
                {
                    // Upload if not exists
                    using var fileStream = File.OpenRead(filePath);
                    var contentType = fileName.EndsWith(".jpg") ? "image/jpeg" : "image/png";
                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(fileStream)
                        .WithObjectSize(fileStream.Length)
                        .WithContentType(contentType));
                }
            }
        }

        // 3. Seed Exercises
        if (!_dbContext.Exercises.Any())
        {
            var exercisesJson = await File.ReadAllTextAsync(Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "exercises.json"));
            var exercisesData = JsonSerializer.Deserialize<List<ExerciseSeedData>>(exercisesJson);
            if (exercisesData != null)
            {
                var exercises = exercisesData.Select(e => new Exercise
                {
                    Name = e.Name,
                    MuscleGroup = e.MuscleGroup,
                    Difficulty = e.Difficulty,
                    Equipment = e.Equipment,
                    Description = e.Description
                }).ToList();

                _dbContext.Exercises.AddRange(exercises);
                await _dbContext.SaveChangesAsync();
            }
        }

        // 4. Seed FoodItems
        if (!_dbContext.FoodItems.Any())
        {
            var foodsJson = await File.ReadAllTextAsync(Path.Combine(_env.ContentRootPath, "Assets", "SeedData", "foods.json"));
            var foodsData = JsonSerializer.Deserialize<List<FoodItemSeedData>>(foodsJson);
            if (foodsData != null)
            {
                var foodItems = foodsData.Select(f => new FoodItem
                {
                    Name = f.Name,
                    ServingSize = f.ServingSize,
                    ServingUnit = f.ServingUnit,
                    CaloriesKcal = f.Calories,
                    ProteinG = f.Protein,
                    CarbsG = f.Carbs,
                    FatG = f.Fat
                }).ToList();

                _dbContext.FoodItems.AddRange(foodItems);
                await _dbContext.SaveChangesAsync();
            }
        }

        // 5. Seed Fake Users for Testing Pagination
        if (_dbContext.ApplicationUsers.Count() < 50)
        {
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
                .RuleFor(u => u.PasswordHash, f => "DUMMY_HASH_FOR_TESTING")
                .RuleFor(u => u.IsActive, f => true);

            // Generate 50 fake users
            var fakeUsers = userFaker.Generate(50);

            foreach (var user in fakeUsers)
            {
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

public class ExerciseSeedData
{
    public required string Name { get; set; }
    public required string MuscleGroup { get; set; }
    public required string Difficulty { get; set; }
    public required string Equipment { get; set; }
    public required string Description { get; set; }
}

public class FoodItemSeedData
{
    public required string Name { get; set; }
    public required int ServingSize { get; set; }
    public required string ServingUnit { get; set; }
    public required decimal Calories { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Fat { get; set; }
}