using Npm.Renovator.ConsoleApp.NpmHttpClient.Models;

namespace Npm.Renovator.ConsoleApp.NpmHttpClient
{
    public record NpmJsRegistryResponse
    {
        public IReadOnlyCollection<NpmJsRegistryResponseSingleObject> Objects { get; init; } = [];
        public long Total { get; init; }
        public DateTime Time { get; init; }
        public required NpmJsRegistryResponseSingleObjectDownloads Downloads { get; init; }
        public required NpmJsRegistryResponseScore Score { get; init; }
        public Dictionary<string, string> Flags { get; init; } = [];
    }
}
