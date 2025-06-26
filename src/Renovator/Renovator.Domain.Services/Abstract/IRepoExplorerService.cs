using Renovator.Domain.Models;

namespace Renovator.Domain.Services.Abstract
{
    internal interface IRepoExplorerService
    {
        Task<IReadOnlyCollection<LazyPackageJson>> AnalyseMultiplePackageJsonDependenciesAsync(string fullFilePathToFolder, CancellationToken cancellationToken = default);
        Task<LazyPackageJson> AnalysePackageJsonDependenciesAsync(string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default);
        Task<LazyPackageJson> UpdateExistingPackageJsonDependenciesAsync(
            LazyPackageJson originalWithNewPackages,
            string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default);
    }
}
