using HealthSync.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// 👉 Bật Swagger cả ở môi trường Development và Production

// Map endpoint
app.MapGet("/health", () => "HealthSync API is running!")
    .WithName("GetHealth")
    .WithOpenApi();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthSync API v1");
    c.RoutePrefix = string.Empty; // truy cập trực tiếp ở http://localhost:5000
});

app.Run();