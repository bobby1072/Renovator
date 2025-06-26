namespace Renovator.NpmHttpClient.Models.Response
{
    public sealed record NpmJsRegistryResponseSingleObjectScore
    {
        public required decimal Final { get; init; }
        public Dictionary<string, decimal> Detail { get; init; } = [];
    }
}