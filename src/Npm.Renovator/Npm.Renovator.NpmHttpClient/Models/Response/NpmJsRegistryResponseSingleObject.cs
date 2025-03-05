namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public sealed record NpmJsRegistryResponseSingleObject
    {
        public required long Dependents { get; init; }
        public required DateTime Updated { get; init; }
        public required decimal SearchScore { get; init; }
        public required NpmJsRegistryResponseSingleObjectPackage Package { get; init; }
        public required NpmJsRegistryResponseSingleObjectDownloads Downloads { get; init; }
        public required NpmJsRegistryResponseSingleObjectScore Score { get; init; }
        public Dictionary<string, string> Flags { get; init; } = [];
    }
}
