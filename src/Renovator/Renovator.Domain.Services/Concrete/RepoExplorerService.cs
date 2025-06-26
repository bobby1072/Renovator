using BT.Common.FastArray.Proto;
using Microsoft.Extensions.Logging;
using Renovator.Domain.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using Renovator.Common.Extensions;
using Renovator.Domain.Services.Abstract;

namespace Renovator.Domain.Services.Concrete
{
    internal sealed class RepoExplorerService : IRepoExplorerService
    {
        private static readonly JsonNodeOptions _jsonNodeOptions =
            new() { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions _jsonSerializerOptionsForPackageJsonWrite =
            new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly ILogger<RepoExplorerService> _logger;

        public RepoExplorerService(ILogger<RepoExplorerService> logger)
        {
            _logger = logger;
        }

        public async Task<
            IReadOnlyCollection<LazyPackageJson>
        > AnalyseMultiplePackageJsonDependenciesAsync(
            string fullFilePathToFolder,
            CancellationToken cancellationToken = default
        )
        {
            var allFiles = Directory
                .EnumerateFiles(fullFilePathToFolder, "package.json", SearchOption.AllDirectories)
                .ToArray();
            if (allFiles.Length == 0)
            {
                return [];
            }

            var files = await Task.WhenAll(
                allFiles.Select(x => ReadJsonFile(x, cancellationToken))
            );
            return files
                .FastArraySelect(x => new LazyPackageJson
                {
                    FullLocalPathToPackageJson = x.FullFilePath,
                    FullPackageJson = CreateLazyFullPackageJson(x.FileText),
                    OriginalPackageJsonDependencies =
                        JsonSerializer.Deserialize<PackageJsonDependencies>(
                            x.FileText,
                            _jsonSerializerOptionsForPackageJsonWrite
                        ) ?? throw new InvalidOperationException("Unable to parse file content"),
                })
                .ToArray();
        }

        public async Task<LazyPackageJson> AnalysePackageJsonDependenciesAsync(
            string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default
        )
        {
            var fileText = await ReadJsonFile(localSystemFilePathToPackageJson, cancellationToken);

            var parsedPackageJsonDependencies =
                JsonSerializer.Deserialize<PackageJsonDependencies>(
                    fileText.FileText,
                    _jsonSerializerOptionsForPackageJsonWrite
                ) ?? throw new InvalidOperationException("Unable to parse file content");

            return new LazyPackageJson
            {
                FullLocalPathToPackageJson = localSystemFilePathToPackageJson,
                FullPackageJson = CreateLazyFullPackageJson(fileText.FileText),
                OriginalPackageJsonDependencies = parsedPackageJsonDependencies,
            };
        }

        public async Task<LazyPackageJson> UpdateExistingPackageJsonDependenciesAsync(
            LazyPackageJson originalWithNewPackages,
            string localSystemFilePathToPackageJson,
            CancellationToken cancellationToken = default
        )
        {
            var jsonObject = originalWithNewPackages.FullPackageJson.Value;

            var updatedJsonObject = jsonObject.UpdateProperties(
                originalWithNewPackages.PotentialNewPackageJsonDependencies
                    ?? throw new ArgumentNullException(
                        nameof(originalWithNewPackages.PotentialNewPackageJsonDependencies)
                    ),
                _jsonSerializerOptionsForPackageJsonWrite,
                _jsonNodeOptions
            );

            await File.WriteAllTextAsync(
                originalWithNewPackages.FullLocalPathToPackageJson,
                updatedJsonObject.ToJsonString(_jsonSerializerOptionsForPackageJsonWrite),
                cancellationToken
            );

            return await AnalysePackageJsonDependenciesAsync(
                localSystemFilePathToPackageJson,
                cancellationToken
            );
        }

        private async Task<(string FileText, string FullFilePath)> ReadJsonFile(
            string filePath,
            CancellationToken cancellationToken
        )
        {
            var fullPath =
                Path.GetFullPath(filePath)
                ?? throw new InvalidOperationException("Unable to find json file");

            if (Path.GetExtension(fullPath) != ".json")
            {
                throw new InvalidOperationException(
                    "Your file path must be pointed at a json file"
                );
            }

            var fileText = await File.ReadAllTextAsync(fullPath, cancellationToken);
            if (string.IsNullOrEmpty(fileText))
            {
                throw new InvalidOperationException("Json file could not be read");
            }

            _logger.LogDebug("Successfully read package json: {PackageJson}", fileText);

            return (fileText, fullPath);
        }

        private static Lazy<JsonObject> CreateLazyFullPackageJson(
            string fileText
        ) =>
            new(
                () =>
                    JsonNode.Parse(fileText, _jsonNodeOptions)?.AsObject()
                    ?? throw new InvalidOperationException("Unable to parse file content")
            );
    }
}
