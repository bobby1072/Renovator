﻿using Npm.Renovator.Domain.Models;

namespace Npm.Renovator.Domain.Services.Abstract
{
    internal interface IRepoReaderService
    {
        /// <summary>
        /// Get dependencies from package json file
        /// </summary>
        /// <param name="localSystemFilePathToPackageJson">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        Task<PackageJsonDependencies> AnalysePackageJsonDependenciesAsync(string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Update dependencies from package json file
        /// </summary>
        /// <param name="localSystemFilePathToPackageJson">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        /// <param name="newPackageJsonDependencies">
        ///     Pass in updated dependencies. This will replace the existing values completely.
        /// </param>
        Task<PackageJsonDependencies> UpdateExistingPackageJsonDependenciesAsync(
            PackageJsonDependencies newPackageJsonDependencies, 
            string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default);
    }
}
