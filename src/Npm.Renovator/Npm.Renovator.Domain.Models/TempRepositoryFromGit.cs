namespace Npm.Renovator.Domain.Models
{
    public record TempRepositoryFromGit
    {
        public required Guid FolderId { get; init; }
        public required string FullPathTo { get; init; }
    }
}
