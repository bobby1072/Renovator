namespace Npm.Renovator.NpmHttpClient.Models.Response
{
    public record NpmJsRegistryResponseSingleObjectUser
    {
        public required string Email { get; init; }
        public required string Username { get; init; }
    }
}