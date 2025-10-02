using HealthSync.Application.Services;
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
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register services
        services.AddScoped<IApplicationUserService, ApplicationUserService>();

        // Add AutoMapper if needed
        // services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}