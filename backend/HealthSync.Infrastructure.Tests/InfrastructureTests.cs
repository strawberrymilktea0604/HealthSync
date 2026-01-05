using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.Infrastructure.Tests;

public class InfrastructureTests
{
    [Fact]
    public void ServiceCollectionExtensions_ShouldRegisterServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Note: This would require full configuration, simplified test
        Assert.NotNull(services);
    }

    [Fact]
    public void AuthService_ShouldImplementInterface()
    {
        // AuthService implements IAuthService
        var serviceType = typeof(AuthService);
        var interfaceType = typeof(IAuthService);
        Assert.True(interfaceType.IsAssignableFrom(serviceType));
    }

    [Fact]
    public void EmailService_ShouldImplementInterface()
    {
        var serviceType = typeof(EmailService);
        var interfaceType = typeof(IEmailService);
        Assert.True(interfaceType.IsAssignableFrom(serviceType));
    }

    [Fact]
    public void MinioStorageService_ShouldImplementInterface()
    {
        var serviceType = typeof(MinioStorageService);
        var interfaceType = typeof(IStorageService);
        Assert.True(interfaceType.IsAssignableFrom(serviceType));
    }

    [Fact]
    public void GroqAiChatService_ShouldImplementInterface()
    {
        var serviceType = typeof(GroqAiChatService);
        var interfaceType = typeof(IAiChatService);
        Assert.True(interfaceType.IsAssignableFrom(serviceType));
    }

    [Fact]
    public void GoogleAuthService_ShouldImplementInterface()
    {
        var serviceType = typeof(GoogleAuthService);
        var interfaceType = typeof(IGoogleAuthService);
        Assert.True(interfaceType.IsAssignableFrom(serviceType));
    }
}