using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.GithubGqlClient.Configuration;

namespace Npm.Renovator.GithubGqlClient.Extensions
{
    public static class GithubGqlClientServiceCollectionExtensions
    {
        public static IServiceCollection AddGithubGqlClient(this IServiceCollection services, IConfiguration configManager) 
        {
            var foundConfigSection = configManager.GetSection(GithubGqlClientSettingsConfiguration.Key);

            if (!foundConfigSection.Exists())
            {
                throw new InvalidDataException($"Environment variables missing for {GithubGqlClientSettingsConfiguration.Key}");
            }

            services.Configure<GithubGqlClientSettingsConfiguration>(foundConfigSection);


            return services;
        }
    }
}
