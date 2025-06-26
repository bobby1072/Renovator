namespace Renovator.NpmHttpClient.Models.Response
{
    public sealed record NpmJsRegistryResponseSingleObjectPackage
    {
        public required string Name { get; init; }
        public required string Version { get; init; }
        public string? Description { get; init; }
        public string? License { get; init; }
        public IReadOnlyCollection<string> Keywords { get; init; } = [];
        public required DateTime Date { get; init; }
        public required NpmJsRegistryResponseSingleObjectUser Publisher { get; init; }
        public IReadOnlyCollection<NpmJsRegistryResponseSingleObjectUser> Maintainers { get; init; } = [];
        public Dictionary<string, string> Links { get; init; } = [];
    }
}