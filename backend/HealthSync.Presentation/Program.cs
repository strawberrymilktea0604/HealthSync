using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);

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
builder.Services.AddSwaggerGen();

var app = builder.Build();


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

app.MapPost("/api/auth/register", async ([FromBody] RegisterRequest request, IMediator mediator) =>
{
    try
    {
        var command = new RegisterUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg
        };

        var userId = await mediator.Send(command);
        return Results.Created($"/api/users/{userId}", new object[] { new { UserId = userId, Message = "Đăng ký thành công!" } });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
    catch (Exception)
    {
        return Results.Problem("Lỗi server nội bộ", statusCode: 500);
    }
})
.WithName("Register")
.WithOpenApi();

// Đăng nhập người dùng
app.MapPost("/api/auth/login", async ([FromBody] LoginRequest request, IMediator mediator) =>
{
    try
    {
        var query = new LoginQuery
        {
            Email = request.Email,
            Password = request.Password
        };

        var response = await mediator.Send(query);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception)
    {
        return Results.Problem("Lỗi server nội bộ", statusCode: 500);
    }
})
.WithName("Login")
.WithOpenApi();

app.Run();

