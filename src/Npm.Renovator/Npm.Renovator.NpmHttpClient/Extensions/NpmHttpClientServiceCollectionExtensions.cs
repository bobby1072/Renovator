using BT.Common.Http.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Concrete;
using Npm.Renovator.NpmHttpClient.Configuration;

namespace Npm.Renovator.NpmHttpClient.Extensions
{
    public static class NpmHttpClientServiceCollectionExtensions
    {
        private static readonly DefaultFlurlJsonSerializer DefaultJsonSerializer = new DefaultFlurlJsonSerializer();
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
                .AddScoped<INpmJsRegistryHttpClient>(sp =>
                {
                    return new NpmJsRegistryHttpClient(
                        sp.GetRequiredService<IOptionsSnapshot<NpmJsRegistryHttpClientSettingsConfiguration>>(),
                        DefaultJsonSerializer,
                        sp.GetRequiredService<ILoggerFactory>().CreateLogger<NpmJsRegistryHttpClient>()
                    );
                });


            return services;
        }
    }
}
