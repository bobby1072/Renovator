using Npm.Renovator.Domain.Models;

namespace Npm.Renovator.Domain.Services.Abstract
{
    internal interface IRepoReaderService
    {
        Task<PackageJsonDependencies> AnalysePackageJsonDependenciesAsync(string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default);
        Task<PackageJsonDependencies> UpdateExistingPackageJsonDependenciesAsync(
            PackageJsonDependencies newPackageJsonDependencies, 
            string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default);
    }
}
