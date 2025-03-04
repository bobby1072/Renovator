namespace Npm.Renovator.NpmHttpClient.Models
{
    public record NpmJsRegistryResponseSingleObjectPackage
    {
        public required string Name { get; init; }
        public required string Version { get; init; }
        public string? Description { get; init; }
        public IReadOnlyCollection<string> Keywords { get; init; } = [];
        public string? License { get; init; }
        public DateTime Date { get; init; }
        public required NpmJsRegistryResponseSingleObjectUser Publisher { get; init; }
        public IReadOnlyCollection<NpmJsRegistryResponseSingleObjectUser> Maintainers { get; init; } = [];
        public Dictionary<string, string> Links { get; init; } = [];
    }
}