using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Concrete;
using Renovator.NpmHttpClient.Extensions;

namespace Renovator.Domain.Services.Extensions;

public static class DomainServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfiguration configurationManager)
    {
        serviceCollection
            .AddLogging()
            .AddHttpClient()
            .AddNpmHttpClient(configurationManager)
            .AddScoped<IComputerResourceCheckerService, ComputerResourceCheckerService>()
            .AddScoped<INpmCommandService, NpmCommandService>()
            .AddScoped<IGitCommandService, GitCommandService>()
            .AddScoped<INpmRenovatorProcessingManager, NpmRenovatorProcessingManager>()
            .AddScoped<IGitNpmRenovatorProcessingManager, GitNpmRenovatorProcessingManager>()
            .AddScoped<IRepoExplorerService, RepoExplorerService>();
        
        return serviceCollection;
    }
}