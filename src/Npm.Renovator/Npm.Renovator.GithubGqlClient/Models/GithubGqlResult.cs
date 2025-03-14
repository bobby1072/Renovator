namespace Npm.Renovator.GithubGqlClient.Models
{
    internal record GithubGqlResult
    {
        public IReadOnlyCollection<GithubGqlError>? Errors { get; init; }
    }
    internal record GithubGqlResult<T> : GithubGqlResult where T : class
    {
        public KeyValuePair<string ,T?>? Data { get; init; }
        public T? ActualValue => Data?.Value;
    }

    internal record GithubGqlResultGenericData: GithubGqlResult
    {
        public Dictionary<string, string?>? Data { get; init; }
    }
}
