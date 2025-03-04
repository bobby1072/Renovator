using Npm.Renovator.RepoReader.Models;

namespace Npm.Renovator.RepoReader.Abstract
{
    public interface IRepoReaderService
    {
        Task<PackageJsonDependencies> AnalysePackageJsonDependencies(string filePath, CancellationToken cancellationToken = default);

        Task<PackageJsonDependencies> UpdateExistingPackageJsonDependencies(
            PackageJsonDependencies newPackageJsonDependencies, string filePath,
            CancellationToken cancellationToken = default);
    }
}
