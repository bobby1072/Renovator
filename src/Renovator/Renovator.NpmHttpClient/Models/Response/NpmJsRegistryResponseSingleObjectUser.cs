namespace Renovator.NpmHttpClient.Models.Response
{
    public sealed record NpmJsRegistryResponseSingleObjectUser
    {
        public required string Email { get; init; }
        public required string Username { get; init; }
    }
}