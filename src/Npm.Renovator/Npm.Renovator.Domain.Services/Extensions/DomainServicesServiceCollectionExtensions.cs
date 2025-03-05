using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Domain.Services.Abstract;
using Npm.Renovator.Domain.Services.Concrete;
using Npm.Renovator.NpmHttpClient.Extensions;

namespace Npm.Renovator.Domain.Services.Extensions;

public static class DomainServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfigurationManager configurationManager)
    {
        serviceCollection
            .AddNpmHttpClient(configurationManager)
            .AddScoped<INpmCommandService, NpmCommandService>()
            .AddScoped<INpmRenovatorProcessingManager, INpmRenovatorProcessingManager>()
            .AddScoped<IRepoReaderService, RepoReaderService>();
        
        return serviceCollection;
    }
}