namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public record NpmJsRegistryResponseSingleObjectScore
    {
        public required decimal Final { get; init; }
        public Dictionary<string, decimal> Detail { get; init; } = [];
    }
}