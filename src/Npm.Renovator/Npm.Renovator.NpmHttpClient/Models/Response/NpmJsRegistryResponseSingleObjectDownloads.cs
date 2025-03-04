namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public record NpmJsRegistryResponseSingleObjectDownloads
    {
        public long Monthly { get; init; }
        public long Weekly { get; init; }
    }
}