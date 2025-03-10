using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Domain.Services.Abstract;
using Npm.Renovator.Domain.Services.Concrete;
using Npm.Renovator.NpmHttpClient.Extensions;

namespace Npm.Renovator.Domain.Services.Extensions;

public static class DomainServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfiguration configurationManager)
    {
        serviceCollection
            .AddNpmHttpClient(configurationManager)
            .AddHostedService<ResourceCheckerService>()
            .AddScoped<INpmCommandService, NpmCommandService>()
            .AddScoped<INpmRenovatorProcessingManager, NpmRenovatorProcessingManager>()
            .AddScoped<IRepoReaderService, RepoReaderService>();
        
        return serviceCollection;
    }
}