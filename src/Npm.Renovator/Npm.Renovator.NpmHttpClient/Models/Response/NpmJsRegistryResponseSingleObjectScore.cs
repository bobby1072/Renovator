namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public record NpmJsRegistryResponseSingleObjectScore
    {
        public decimal Final { get; init; }
        public Dictionary<string, decimal> Detail { get; init; } = [];
    }
}