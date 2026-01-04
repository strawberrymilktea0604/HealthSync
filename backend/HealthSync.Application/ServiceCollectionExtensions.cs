using HealthSync.Application.Services;
using HealthSync.Application.Common.Behaviors;
using HealthSync.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace HealthSync.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add MediatR with Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register AuditLogBehavior để tự động log mọi Command
            cfg.AddOpenBehavior(typeof(AuditLogBehavior<,>));
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register services
        services.AddScoped<IApplicationUserService, ApplicationUserService>();

        // Add AutoMapper if needed

        return services;
    }
}