using BT.Common.Polly.Models.Concrete;

namespace Renovator.NpmHttpClient.Configuration
{
    internal sealed record NpmJsRegistryHttpClientSettingsConfiguration: PollyRetrySettings
    {
        public const string Key = "NpmJsRegistryHttpClientSettings";
        public required string BaseUrl {  get; init; }
    }
}
