namespace Npm.Renovator.ConsoleApp.NpmHttpClient.Models
{
    public record NpmJsRegistryResponseSingleObjectDownloads
    {
        public long Monthly { get; init; }
        public long Weekly { get; init; }
    }
}