using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// 👉 Bật Swagger cả ở môi trường Development và Production

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthSync API v1");
    c.RoutePrefix = string.Empty; // truy cập trực tiếp ở http://localhost:5000
});

// Map endpoint
app.MapGet("/health", () => "HealthSync API is running!")
    .WithName("GetHealth")
    .WithOpenApi();

// Authentication endpoints
app.MapPost("/api/auth/register", async (RegisterRequest request, IMediator mediator) =>
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
        return Results.Created($"/api/users/{userId}", new { UserId = userId, Message = "Đăng ký thành công!" });
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

app.MapPost("/api/auth/login", async (LoginRequest request, IMediator mediator) =>
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