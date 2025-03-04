namespace Npm.Renovator.NpmHttpClient.Models.Request
{
    public record NpmJsRegistryRequestBody
    {
        public required string Text { get; init; }
        public required int Size {  get; init; }
    }
}
