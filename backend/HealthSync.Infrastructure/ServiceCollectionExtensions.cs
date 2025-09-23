using HealthSync.Domain.Interfaces;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthSync.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<HealthSyncDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        // Add JWT Authentication
        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}