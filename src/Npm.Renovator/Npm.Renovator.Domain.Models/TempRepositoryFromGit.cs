using Npm.Renovator.Common.Helpers;

namespace Npm.Renovator.Domain.Models
{
    public sealed record TempRepositoryFromGit : IDisposable
    {
        public required Uri GitRepoLocation { get; init; }
        public Guid FolderId { get; init; } = Guid.NewGuid();
        public required string FullPathTo { get; init; }
        public void Dispose()
        {
            Task.Run(() => FileHelper.EnsureDeleted(FullPathTo));
        }
    }
}
