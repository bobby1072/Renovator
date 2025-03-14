using BT.Common.Polly.Models.Concrete;

namespace Npm.Renovator.GithubGqlClient.Configuration
{
    internal record GithubGqlClientSettingsConfiguration: PollyRetrySettings
    {
        public const string Key = "GithubGqlClientSettings";
        public required string BaseUrl { get; init; }
    }
}
