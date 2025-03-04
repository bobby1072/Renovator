namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public record NpmJsRegistryResponseSingleObject
    {
        public long Dependents { get; init; }
        public DateTime Updated { get; init; }
        public decimal SearchScore { get; init; }
        public required NpmJsRegistryResponseSingleObjectPackage Package { get; init; }
        public required NpmJsRegistryResponseSingleObjectDownloads Downloads { get; init; }
        public required NpmJsRegistryResponseSingleObjectScore Score { get; init; }
    }
}
