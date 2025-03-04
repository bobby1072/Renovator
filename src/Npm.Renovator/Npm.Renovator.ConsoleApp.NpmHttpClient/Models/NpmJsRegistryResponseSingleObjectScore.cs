namespace Npm.Renovator.ConsoleApp.NpmHttpClient.Models
{
    public record NpmJsRegistryResponseSingleObjectScore
    {
        public decimal Final { get; init; }
        public Dictionary<string, decimal> Detail { get; init; } = [];
    }
}