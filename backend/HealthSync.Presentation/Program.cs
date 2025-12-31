using HealthSync.Application;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Infrastructure;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Presentation.Middleware;
using HealthSync.Presentation.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Polly;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Load .env file - check multiple locations
var rootEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
var projectEnvPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

if (File.Exists(rootEnvPath))
{
    DotNetEnv.Env.Load(rootEnvPath);
    Console.WriteLine($"✅ Loaded .env from: {rootEnvPath}");
}
else if (File.Exists(projectEnvPath))
{
    DotNetEnv.Env.Load(projectEnvPath);
    Console.WriteLine($"✅ Loaded .env from: {projectEnvPath}");
}
else
{
    Console.WriteLine("⚠️ No .env file found, using appsettings or Docker environment variables");
}

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod(); 
    });
});


// Cấu hình API Explorer và Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HealthSync API",
        Version = "v1",
        Description = "API for HealthSync - Health & Fitness Tracking System",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "HealthSync Team",
            Email = "support@healthsync.com"
        }
    });

    // Handle file uploads
    options.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    // Cấu hình JWT Bearer Authentication cho Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token below.\n\nExample: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'\n\nDo not add 'Bearer' prefix."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Use camelCase for JS/React
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    });

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = false;
});

var app = builder.Build();

// Global exception handler
app.UseMiddleware<GlobalExceptionHandler>();

// Bật CORS sớm trong pipeline để xử lý Preflight (OPTIONS) request
// Buộc sử dụng DevCorsPolicy để giải quyết lỗi 405/CORS
app.UseCors("DevCorsPolicy");
app.Use(async (context, next) =>
{
    // Kiểm tra nếu là request OPTIONS và CORS đã xử lý thành công (hoặc đang chờ xử lý)
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        await context.Response.CompleteAsync();
        return;
    }
    await next(context);
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthSync API v1");
    c.RoutePrefix = string.Empty; 
});

// Middleware xác thực và ủy quyền
app.UseAuthentication();
app.UseAuthorization();

// Endpoint kiểm tra sức khỏe của API
app.MapGet("/health", () => "HealthSync API is running!")
    .WithName("GetHealth");

app.MapControllers();

// Migration tự động với Retry Policy + Random Jitter để tránh Race Condition
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<HealthSyncDbContext>();

    try
    {
        logger.LogInformation("⏳ Waiting for SQL Server to be ready...");

        // 1. Random Jitter: Ngủ ngẫu nhiên 1-5 giây để 2 container không chạy đồng thời
        var random = new Random();
        int delay = random.Next(1000, 5000);
        logger.LogInformation("🎲 Random delay: {Delay}ms before migration attempt", delay);
        await Task.Delay(delay);

        // 2. Định nghĩa Retry Policy (Thử lại tối đa 5 lần)
        var retryPolicy = Policy
            .Handle<SqlException>(ex => 
                ex.Number == 1801 || // Database already exists
                ex.Number == 4060 || // Cannot open database (đang tạo dở)
                ex.Number == 18456 || // Login failed (SQL chưa kịp mount DB)
                ex.Number == 1205    // Deadlock victim
            )
            .Or<InvalidOperationException>() // EF Core exceptions
            .Or<Exception>(ex => 
                ex.Message.Contains("already exists") || 
                ex.Message.Contains("Cannot open database") ||
                ex.Message.Contains("login failed", StringComparison.OrdinalIgnoreCase)
            )
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s, 16s, 32s
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        "⚠️ Migration attempt {RetryCount} failed: {Message}. Waiting {Seconds}s before retry...",
                        retryCount, 
                        exception.Message, 
                        timeSpan.TotalSeconds
                    );
                }
            );

        // 3. Thực thi Migration với Retry Policy
        await retryPolicy.ExecuteAsync(async () =>
        {
            logger.LogInformation("🚀 Starting database migration...");
            
            // Check if using in-memory database (for tests)
            if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                logger.LogInformation("ℹ️ In-memory database detected, skipping migrations.");
            }
            else
            {
                // MigrateAsync tự động check bảng __EFMigrationsHistory
                // Nếu DB đã tồn tại và migrations đã chạy, nó sẽ skip
                await context.Database.MigrateAsync();
                logger.LogInformation("✅ Database migration completed successfully!");
            }
        });

        // 4. Seed data (DataSeeder có lock riêng nên an toàn với 2 instances)
        // Skip seeding for in-memory databases (used in tests)
        if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }
    }
    catch (Exception ex)
    {
        // Log lỗi nhưng KHÔNG crash app (để container không bị restart loop)
        logger.LogError(ex, "❌ Failed to initialize database after multiple retry attempts.");
        logger.LogWarning("⚠️ Application will continue running, but may not function correctly without database.");
        
        // Tùy chọn: Throw để container restart, hoặc để chạy tiếp (API sẽ lỗi khi query DB)
        // throw; // Uncomment nếu muốn container restart khi migration fail
    }
}

await app.RunAsync();

namespace HealthSync.Presentation
{
    // Make Program class accessible to integration tests
    public partial class Program { protected Program() { } }

    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Type == typeof(IFormFile))
                .ToList();

            if (fileParameters.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["file"] = new OpenApiSchema { Type = "string", Format = "binary" }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
