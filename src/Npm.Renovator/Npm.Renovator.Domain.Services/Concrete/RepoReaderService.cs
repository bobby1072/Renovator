using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Extensions;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class RepoReaderService: IRepoReaderService
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptionsForPackageJsonWrite = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly ILogger<RepoReaderService> _logger;

        public RepoReaderService(ILogger<RepoReaderService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get dependencies from package json file
        /// </summary>
        /// <param name="localSystemFilePathToPackageJson">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        public async Task<PackageJsonDependencies> AnalysePackageJsonDependenciesAsync(string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
        {
            var fileText = await ReadJsonFile(localSystemFilePathToPackageJson, cancellationToken);
            
            var parsedPackageJsonDependencies = JsonSerializer.Deserialize<PackageJsonDependencies>(fileText.FileText, _jsonSerializerOptionsForPackageJsonWrite)
                ?? throw new InvalidOperationException("Unable to parse file content");
            
            return parsedPackageJsonDependencies;
        }
        /// <summary>
        /// Update dependencies from package json file
        /// </summary>
        /// <param name="newPackageJsonDependencies">
        ///     Pass in updated dependencies. This will replace the existing values completely.
        /// </param>
        /// <param name="localSystemFilePathToPackageJson">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        public async Task<PackageJsonDependencies> UpdateExistingPackageJsonDependenciesAsync(
            PackageJsonDependencies newPackageJsonDependencies, string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
        {
            var fileText = await ReadJsonFile(localSystemFilePathToPackageJson, cancellationToken);

            var jsonObject = JsonNode.Parse(fileText.FileText)!.AsObject()
                                  ?? throw new InvalidOperationException("Unable to parse file content");
            
            var updatedJsonObject = jsonObject.UpdateProperties(newPackageJsonDependencies, _jsonSerializerOptionsForPackageJsonWrite);

            await File.WriteAllTextAsync(updatedJsonObject.ToJsonString(_jsonSerializerOptionsForPackageJsonWrite),
                fileText.FullFilePath, cancellationToken);

            
            return await AnalysePackageJsonDependenciesAsync(localSystemFilePathToPackageJson, cancellationToken);
        }

        private async Task<(string FileText, string FullFilePath)> ReadJsonFile(string filePath, CancellationToken cancellationToken)
        {
            var fullPath = Path.GetFullPath(filePath) ?? throw new InvalidOperationException("Unable to find json file");

            if (Path.GetExtension(fullPath) != ".json")
            {
                throw new InvalidOperationException("Your file path must be pointed at a json file");
            }

            var fileText = await File.ReadAllTextAsync(fullPath, cancellationToken);
            if (string.IsNullOrEmpty(fileText))
            {
                throw new InvalidOperationException("Json file could not be read");
            }
            
            _logger.LogDebug("Successfully read package json dependencies: {Dependencies}", fileText);
            
            return (fileText, fullPath);
        }

    }
}
