using Npm.Renovator.Common.Helpers;

namespace Npm.Renovator.Domain.Models
{
    public record TempRepositoryFromGit : IDisposable
    {
        public required Uri GitRepoLocation { get; init; }
        public required Guid FolderId { get; init; }
        public required string FullPathTo { get; init; }
        public void Dispose()
        {
            Task.Run(() => FileHelper.EnsureDeleted(FullPathTo));
        }
    }
}
