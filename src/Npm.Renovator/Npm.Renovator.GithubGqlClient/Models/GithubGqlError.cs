namespace Npm.Renovator.GithubGqlClient.Models
{
    internal record GithubGqlError
    {
        public required string Type { get; init; }
        public IReadOnlyCollection<string> Path { get; init; } = [];
        public IReadOnlyCollection<GithubGqlErrorLocation> Locations { get; init; } = [];
        public required string Message { get; init; }
    }
    internal record GithubGqlErrorLocation
    {
        public int Line { get; init; }
        public int Column { get; init; }
    }
}
