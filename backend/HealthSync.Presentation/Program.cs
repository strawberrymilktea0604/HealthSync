using HealthSync.Application;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Infrastructure;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Presentation.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

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

    // Cấu hình JWT Bearer Authentication cho Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token vào ô bên dưới.\n\nVí dụ: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'\n\nKhông cần thêm từ 'Bearer' phía trước."
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
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    });

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
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
    .WithName("GetHealth")
    .WithOpenApi();

app.MapControllers();

// Migration tự động khi khởi chạy Docker
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HealthSyncDbContext>();
        // Lệnh này sẽ tự động check, nếu chưa có DB thì tạo, chưa có bảng thì thêm
        // Nếu có rồi thì thôi, không báo lỗi Crash app như lệnh CreateDatabase
        await context.Database.MigrateAsync(); 
    }
    catch (Exception ex)
    {
        // Log lỗi ra nhưng KHÔNG làm crash app
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        
        // Mẹo: Nếu lỗi "Already exists" thì coi như thành công, cho chạy tiếp
    }
}

await app.RunAsync();

// Make Program class accessible to integration tests
public partial class Program { protected Program() { } }
