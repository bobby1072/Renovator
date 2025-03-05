using Npm.Renovator.Repo.Services.Models;

namespace Npm.Renovator.Repo.Services.Abstract
{
    public interface IRepoReaderService
    {
        /// <summary>
        /// Get dependencies from package json file
        /// </summary>
        /// <param name="filePath">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        Task<PackageJsonDependencies> AnalysePackageJsonDependenciesAsync(string filePath,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Update dependencies from package json file
        /// </summary>
        /// <param name="filePath">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        /// <param name="newPackageJsonDependencies">
        ///     Pass in updated dependencies. This will replace the existing values completely.
        /// </param>
        Task<PackageJsonDependencies> UpdateExistingPackageJsonDependenciesAsync(
            PackageJsonDependencies newPackageJsonDependencies, 
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
