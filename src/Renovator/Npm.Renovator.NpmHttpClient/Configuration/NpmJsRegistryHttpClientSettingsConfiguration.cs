using BT.Common.Polly.Models.Concrete;

namespace Npm.Renovator.NpmHttpClient.Configuration
{
    internal record NpmJsRegistryHttpClientSettingsConfiguration: PollyRetrySettings
    {
        public const string Key = "NpmJsRegistryHttpClientSettings";
        public required string BaseUrl {  get; init; }
    }
}
