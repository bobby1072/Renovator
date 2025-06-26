namespace Renovator.NpmHttpClient.Models.Response
{
    public sealed record NpmJsRegistryResponse
    {
        public IReadOnlyCollection<NpmJsRegistryResponseSingleObject> Objects { get; init; } = [];
        public long Total { get; init; }
        public DateTime Time { get; init; }
    }
}
