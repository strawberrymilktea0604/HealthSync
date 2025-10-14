using HealthSync.Domain.Interfaces;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace HealthSync.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<HealthSyncDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        // services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        // Register DbContext interface
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<HealthSyncDbContext>());

        // Register AuthService
        services.AddScoped<IAuthService, AuthService>();

        // Register EmailService
        services.AddScoped<IEmailService, EmailService>();

        // Register GoogleAuthService
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddHttpClient<GoogleAuthService>();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-secret-key-here";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "HealthSync",
                ValidAudience = jwtSettings["Audience"] ?? "HealthSyncUsers",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        services.AddAuthorization();

        return services;
    }
}