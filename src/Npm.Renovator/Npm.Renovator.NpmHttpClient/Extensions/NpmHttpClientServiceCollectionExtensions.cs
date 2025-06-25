using BT.Common.Http.Extensions;
using BT.Common.Polly.Models.Concrete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Concrete;
using Npm.Renovator.NpmHttpClient.Configuration;

namespace Npm.Renovator.NpmHttpClient.Extensions
{
    public static class NpmHttpClientServiceCollectionExtensions
    {
        public static IServiceCollection AddNpmHttpClient(this IServiceCollection services,
            IConfiguration configManager) 
        {
            var npmApiHttpSettingsSection = configManager.GetSection(NpmJsRegistryHttpClientSettingsConfiguration.Key);

            if (!npmApiHttpSettingsSection.Exists())
            {
                throw new InvalidDataException($"Environment variables missing for {NpmJsRegistryHttpClientSettingsConfiguration.Key}");
            }

            services.Configure<NpmJsRegistryHttpClientSettingsConfiguration>(npmApiHttpSettingsSection);

            services
                .AddHttpClientWithResilience<INpmJsRegistryHttpClient, NpmJsRegistryHttpClient>(npmApiHttpSettingsSection.Get<PollyRetrySettings>() 
                    ?? throw new InvalidDataException($"Environment variables missing for {NpmJsRegistryHttpClientSettingsConfiguration.Key}"));


            return services;
        }
    }
}
