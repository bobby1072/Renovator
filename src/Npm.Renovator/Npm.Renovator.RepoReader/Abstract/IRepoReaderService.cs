using Npm.Renovator.RepoReader.Models;

namespace Npm.Renovator.RepoReader.Abstract
{
    public interface IRepoReaderService
    {
        /// <summary>
        /// Get dependencies from package json file
        /// </summary>
        /// <param name="filePath">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        Task<PackageJsonDependencies> AnalysePackageJsonDependencies(string filePath, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update dependencies from package json file
        /// </summary>
        /// <param name="filePath">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        /// <param name="newPackageJsonDependencies">
        ///     Pass in updated dependencies. This will replace the existing values completely.
        /// </param>
        Task<PackageJsonDependencies> UpdateExistingPackageJsonDependencies(
            PackageJsonDependencies newPackageJsonDependencies, string filePath,
            CancellationToken cancellationToken = default);
    }
}
