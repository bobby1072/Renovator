using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.NpmHttpClient.Extensions;
using Npm.Renovator.RepoReader.Extensions;

namespace Npm.Renovator.Application.Extensions
{
    public static class RenovatorApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfigurationManager configManager)
        {
            serviceCollection
                .AddNpmHttpClient(configManager)
                .AddRepoReader();

            return serviceCollection;
        }
    }
}
