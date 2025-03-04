using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.NpmHttpClient.Extensions;

namespace Npm.Renovator.Application.Extensions
{
    public static class RenovatorApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfigurationManager configManager)
        {
            serviceCollection
                .AddNpmHttpClient(configManager);

            return serviceCollection;
        }
    }
}
