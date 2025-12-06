using HealthSync.Application.Services;
using HealthSync.Domain.Constants;
using HealthSync.Domain.Interfaces;
using HealthSync.Infrastructure.Authorization;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        
        // Register AI Chat Service
        services.AddSingleton<IAiChatService, GeminiAiChatService>();

        // Register DbContext interface
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<HealthSyncDbContext>());

        // Register AuthService
        services.AddScoped<IAuthService, AuthService>();

        // Register JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register EmailService
        services.AddScoped<IEmailService, EmailService>();

        // Register GoogleAuthService
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddHttpClient<GoogleAuthService>();

        // Register StorageService (MinIO)
        services.AddScoped<IStorageService, MinioStorageService>();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        
        // Try to get JWT secret from environment variable first, then config
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                       ?? jwtSettings["SecretKey"] 
                       ?? Environment.GetEnvironmentVariable("JwtSettings__SecretKey")
                       ?? throw new InvalidOperationException("JWT SecretKey not configured. Set JWT_SECRET_KEY environment variable or JwtSettings:SecretKey in appsettings.json");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "HealthSync",
                ValidAudience = jwtSettings["Audience"] ?? "HealthSyncUsers",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add Authorization with custom policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
        services.AddAuthorization(options =>
        {
            // Default policy - require authenticated user
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Fallback policy
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}