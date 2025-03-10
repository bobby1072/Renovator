using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Extensions;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class RepoExplorerService: IRepoExplorerService
    {
        private static readonly JsonNodeOptions _jsonNodeOptions = new() { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions _jsonSerializerOptionsForPackageJsonWrite = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly ILogger<RepoExplorerService> _logger;

        public RepoExplorerService(ILogger<RepoExplorerService> logger)
        {
            _logger = logger;
        }
        public async Task<PackageJsonDependencies> AnalysePackageJsonDependenciesAsync(string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
        {
            var fileText = await ReadJsonFile(localSystemFilePathToPackageJson, cancellationToken);

            var parsedPackageJsonDependencies = JsonSerializer.Deserialize<PackageJsonDependencies>(fileText.FileText, _jsonSerializerOptionsForPackageJsonWrite)
                ?? throw new InvalidOperationException("Unable to parse file content");

            return parsedPackageJsonDependencies;
        }
        public async Task<PackageJsonDependencies> UpdateExistingPackageJsonDependenciesAsync(
            PackageJsonDependencies newPackageJsonDependencies, string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
        {
            var fileText = await ReadJsonFile(localSystemFilePathToPackageJson, cancellationToken);

            var jsonObject = JsonNode.Parse(fileText.FileText, _jsonNodeOptions)!.AsObject()
                                  ?? throw new InvalidOperationException("Unable to parse file content");
            
            var updatedJsonObject = jsonObject.UpdateProperties(newPackageJsonDependencies, _jsonSerializerOptionsForPackageJsonWrite, _jsonNodeOptions);

            await File.WriteAllTextAsync(fileText.FullFilePath, updatedJsonObject.ToJsonString(_jsonSerializerOptionsForPackageJsonWrite),
                 cancellationToken);

            
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
            
            _logger.LogDebug("Successfully read package json: {PackageJson}", fileText);
            
            return (fileText, fullPath);
        }

    }
}
