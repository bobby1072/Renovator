namespace Npm.Renovator.ConsoleApp.NpmHttpClient
{
    public record NpmJsRegistryResponseSingleWholePackage
    {
        public int Dependents { get; init; }

        public DateTime Updated { get; init; }

        public double SearchScore { get; init; }
        public string? License { get; init; }
    }
}
