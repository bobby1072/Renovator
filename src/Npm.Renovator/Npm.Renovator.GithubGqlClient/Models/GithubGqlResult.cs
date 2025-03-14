namespace Npm.Renovator.GithubGqlClient.Models
{
    internal record GithubGqlResult
    {
        public IReadOnlyCollection<GithubGqlError>? Errors { get; init; }
    }
    internal record GithubGqlResult<T> where T : class
    {
        public KeyValuePair<string ,T?>? Data { get; init; }
        public T? ActualValue => Data?.Value;
    }
}
