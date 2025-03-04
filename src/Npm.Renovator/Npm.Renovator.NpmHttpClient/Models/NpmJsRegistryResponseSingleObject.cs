using Npm.Renovator.NpmHttpClient.Models;

namespace Npm.Renovator.NpmHttpClient
{
    public record NpmJsRegistryResponseSingleObject
    {
        public long Dependents { get; init; }
        public DateTime Updated { get; init; }
        public decimal SearchScore { get; init; }
        public string? License { get; init; }
        public required NpmJsRegistryResponseSingleObjectPackage Package { get; init; }
        public required NpmJsRegistryResponseSingleObjectDownloads Downloads { get; init; }
        public required NpmJsRegistryResponseSingleObjectScore Score { get; init; }
        public Dictionary<string, string> Flags { get; init; } = [];
    }
}
