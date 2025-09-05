using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Concrete;
using Renovator.Domain.Services.Extensions;

namespace Renovator.Tests.DomainServicesTests;

public class DomainServicesServiceCollectionExtensionsTests
{
    private static readonly Dictionary<string, string?> _inMemSettings = new ()
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

        // Assert - Verify services are registered as Scoped with correct implementation types
        var computerResourceChecker = services.FirstOrDefault(s => s.ServiceType == typeof(IComputerResourceCheckerService));
        var npmCommandService = services.FirstOrDefault(s => s.ServiceType == typeof(INpmCommandService));
        var gitCommandService = services.FirstOrDefault(s => s.ServiceType == typeof(IGitCommandService));
        var npmRenovatorProcessingManager = services.FirstOrDefault(s => s.ServiceType == typeof(INpmRenovatorProcessingManager));
        var gitNpmRenovatorProcessingManager = services.FirstOrDefault(s => s.ServiceType == typeof(IGitNpmRenovatorProcessingManager));
        var repoExplorerService = services.FirstOrDefault(s => s.ServiceType == typeof(IRepoExplorerService));

        Assert.NotNull(computerResourceChecker);
        Assert.Equal(ServiceLifetime.Scoped, computerResourceChecker.Lifetime);
        Assert.Equal(typeof(ComputerResourceCheckProcessCommand), computerResourceChecker.ImplementationType);

        Assert.NotNull(npmCommandService);
        Assert.Equal(ServiceLifetime.Scoped, npmCommandService.Lifetime);
        Assert.Equal(typeof(NpmInstallProcessCommand), npmCommandService.ImplementationType);

        Assert.NotNull(gitCommandService);
        Assert.Equal(ServiceLifetime.Scoped, gitCommandService.Lifetime);
        Assert.Equal(typeof(CheckoutRemoteRepoToLocalTempStoreProcessCommand), gitCommandService.ImplementationType);

        Assert.NotNull(npmRenovatorProcessingManager);
        Assert.Equal(ServiceLifetime.Scoped, npmRenovatorProcessingManager.Lifetime);
        Assert.Equal(typeof(NpmRenovatorProcessingManager), npmRenovatorProcessingManager.ImplementationType);

        Assert.NotNull(gitNpmRenovatorProcessingManager);
        Assert.Equal(ServiceLifetime.Scoped, gitNpmRenovatorProcessingManager.Lifetime);
        Assert.Equal(typeof(GitNpmRenovatorProcessingManager), gitNpmRenovatorProcessingManager.ImplementationType);

        Assert.NotNull(repoExplorerService);
        Assert.Equal(ServiceLifetime.Scoped, repoExplorerService.Lifetime);
        Assert.Equal(typeof(RepoExplorerService), repoExplorerService.ImplementationType);
    }
}
