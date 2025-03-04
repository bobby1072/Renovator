using Npm.Renovator.NpmHttpClient.Models;

namespace Npm.Renovator.NpmHttpClient
{
    public record NpmJsRegistryResponse
    {
        public IReadOnlyCollection<NpmJsRegistryResponseSingleObject> Objects { get; init; } = [];
        public long Total { get; init; }
        public DateTime Time { get; init; }
        public Dictionary<string, string> Flags { get; init; } = [];
    }
}
