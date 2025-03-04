namespace Npm.Renovator.ConsoleApp.NpmHttpClient
{
    public record NpmJsRegistryResponse
    {
        public IReadOnlyCollection<object> Objects { get; init; } = [];

        public int Total { get; init; }

        public DateTime Time { get; init; }
    }
}
