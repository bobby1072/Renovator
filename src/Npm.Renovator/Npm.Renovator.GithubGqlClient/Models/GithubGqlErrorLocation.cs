namespace Npm.Renovator.GithubGqlClient.Models
{
    internal record GithubGqlErrorLocation
    {
        public required int Line { get; init; }
        public required int Column { get; init; }
    }
}
