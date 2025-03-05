using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Npm.Renovator.Repo.Services.Abstract;
using Npm.Renovator.Repo.Services.Models;

namespace Npm.Renovator.Repo.Services.Concrete
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
        /// <param name="filePath">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        public async Task<PackageJsonDependencies> AnalysePackageJsonDependenciesAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var fileText = await ReadJsonFile(filePath, cancellationToken);
            
            var parsedPackageJsonDependencies = JsonSerializer.Deserialize<PackageJsonDependencies>(fileText.FileText)
                ?? throw new InvalidOperationException("Unable to parse file content");
            
            return parsedPackageJsonDependencies;
        }
        /// <summary>
        /// Update dependencies from package json file
        /// </summary>
        /// <param name="newPackageJsonDependencies">
        ///     Pass in updated dependencies. This will replace the existing values completely.
        /// </param>
        /// <param name="filePath">
        ///     Pass in any file path (including %FileName%.json at the end) and we will analyse dependencies. File does not need to be named "package.json" 
        /// </param>
        public async Task<PackageJsonDependencies> UpdateExistingPackageJsonDependenciesAsync(
            PackageJsonDependencies newPackageJsonDependencies, string filePath, CancellationToken cancellationToken = default)
        {
            var fileText = await ReadJsonFile(filePath, cancellationToken);

            var jsonObject = JsonNode.Parse(fileText.FileText)!.AsObject()
                                  ?? throw new InvalidOperationException("Unable to parse file content");
            
            var updatedJsonObject = UpdateProperties(jsonObject, newPackageJsonDependencies);

            await File.WriteAllTextAsync(updatedJsonObject.ToJsonString(_jsonSerializerOptionsForPackageJsonWrite),
                fileText.FullFilePath, cancellationToken);

            
            return await AnalysePackageJsonDependenciesAsync(filePath, cancellationToken);
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
        private static JsonObject UpdateProperties<T>(JsonObject jsonObject, T objectToUpdateWith) where T : class
        {
            var typeProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in typeProperties)
            {
                var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                
                jsonObject[propertyName] = JsonSerializer.Serialize(property.GetValue(objectToUpdateWith), _jsonSerializerOptionsForPackageJsonWrite);
            }
            
            return jsonObject;
        }
    }
}
