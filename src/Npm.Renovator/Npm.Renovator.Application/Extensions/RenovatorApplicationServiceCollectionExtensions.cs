using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Application.Services.Abstract;
using Npm.Renovator.Application.Services.Concrete;
using Npm.Renovator.NpmHttpClient.Extensions;
using Npm.Renovator.Repo.Services.Extensions;

namespace Npm.Renovator.Application.Extensions
{
    public static class RenovatorApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfigurationManager configManager)
        {
            serviceCollection
                .AddNpmHttpClient(configManager)
                .AddRepoServices()
                .AddScoped<INpmRenovatorProcessingManager, NpmRenovatorProcessingManager>();

            return serviceCollection;
        }
    }
}
