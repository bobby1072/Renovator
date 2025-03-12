namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public record NpmJsRegistryResponse
    {
        public IReadOnlyCollection<NpmJsRegistryResponseSingleObject> Objects { get; init; } = [];
        public required long Total { get; init; }
        public required DateTime Time { get; init; }
    }
}
