namespace Renovator.NpmHttpClient.Models.Response
{
    public sealed record NpmJsRegistryResponseSingleObjectDownloads
    {
        public long Monthly { get; init; }
        public long Weekly { get; init; }
    }
}