namespace Renovator.NpmHttpClient.Models.Request
{
    public sealed record NpmJsRegistryRequestBody
    {
        public required string Text { get; init; }
        public required int Size { get; init; }
    }
}
