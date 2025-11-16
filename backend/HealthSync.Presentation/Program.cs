using HealthSync.Application;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Infrastructure;
using HealthSync.Presentation.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
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

app.Run();

