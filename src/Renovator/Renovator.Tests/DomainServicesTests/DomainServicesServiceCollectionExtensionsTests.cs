using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Extensions;

namespace Renovator.Tests.DomainServicesTests;

public class DomainServicesServiceCollectionExtensionsTests
{
    private static readonly Dictionary<string, string?> _inMemSettings = new Dictionary<string, string?>
        {
            { "NpmJsRegistryHttpClientSettings:BaseUrl", "https://registry.npmjs.com/" },
            { "NpmJsRegistryHttpClientSettings:TimeoutInSeconds", "45" },
            { "NpmJsRegistryHttpClientSettings:TotalAttempts", "3" },
            { "NpmJsRegistryHttpClientSettings:DelayBetweenAttemptsInSeconds", "1" },
            { "NpmJsRegistryHttpClientSettings:UseJitter", "true" }
        };

    [Fact]
    public void AddRenovatorApplication_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemSettings)
            .Build();

        // Act
        services.AddRenovatorApplication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify all services are registered
        Assert.NotNull(serviceProvider.GetService<IComputerResourceCheckerService>());
        Assert.NotNull(serviceProvider.GetService<INpmCommandService>());
        Assert.NotNull(serviceProvider.GetService<IGitCommandService>());
        Assert.NotNull(serviceProvider.GetService<INpmRenovatorProcessingManager>());
        Assert.NotNull(serviceProvider.GetService<IGitNpmRenovatorProcessingManager>());
        Assert.NotNull(serviceProvider.GetService<IRepoExplorerService>());
    }

    [Fact]
    public void AddRenovatorApplication_ShouldRegisterServicesWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemSettings)
            .Build();


        // Act
        services.AddRenovatorApplication(configuration);

        // Assert - Verify services are registered as Scoped
        var computerResourceChecker = services.FirstOrDefault(s => s.ServiceType == typeof(IComputerResourceCheckerService));
        var npmCommandService = services.FirstOrDefault(s => s.ServiceType == typeof(INpmCommandService));
        var gitCommandService = services.FirstOrDefault(s => s.ServiceType == typeof(IGitCommandService));
        var npmRenovatorProcessingManager = services.FirstOrDefault(s => s.ServiceType == typeof(INpmRenovatorProcessingManager));
        var gitNpmRenovatorProcessingManager = services.FirstOrDefault(s => s.ServiceType == typeof(IGitNpmRenovatorProcessingManager));
        var repoExplorerService = services.FirstOrDefault(s => s.ServiceType == typeof(IRepoExplorerService));

        Assert.NotNull(computerResourceChecker);
        Assert.Equal(ServiceLifetime.Scoped, computerResourceChecker.Lifetime);

        Assert.NotNull(npmCommandService);
        Assert.Equal(ServiceLifetime.Scoped, npmCommandService.Lifetime);

        Assert.NotNull(gitCommandService);
        Assert.Equal(ServiceLifetime.Scoped, gitCommandService.Lifetime);

        Assert.NotNull(npmRenovatorProcessingManager);
        Assert.Equal(ServiceLifetime.Scoped, npmRenovatorProcessingManager.Lifetime);

        Assert.NotNull(gitNpmRenovatorProcessingManager);
        Assert.Equal(ServiceLifetime.Scoped, gitNpmRenovatorProcessingManager.Lifetime);

        Assert.NotNull(repoExplorerService);
        Assert.Equal(ServiceLifetime.Scoped, repoExplorerService.Lifetime);
    }

    [Fact]
    public void AddRenovatorApplication_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemSettings)
            .Build();

        // Act
        var result = services.AddRenovatorApplication(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddRenovatorApplication_ShouldRegisterLoggingAndHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemSettings)
            .Build();

        // Act
        services.AddRenovatorApplication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify logging and HTTP client are available
        Assert.NotNull(serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>());
        Assert.NotNull(serviceProvider.GetService<System.Net.Http.IHttpClientFactory>());
    }
}
