using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Renovator.Domain.Models;
using Renovator.Domain.Services.Concrete;
using System.Text.Json;

namespace Renovator.Tests.DomainServicesTests;

public class RepoExplorerServiceTests : IDisposable
{
    private readonly RepoExplorerService _service;
    private readonly string _tempDirectory;

    public RepoExplorerServiceTests()
    {
        _service = new RepoExplorerService(NullLogger<RepoExplorerService>.Instance);
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithValidPackageJson_ShouldReturnLazyPackageJson()
    {
        // Arrange
        var packageJsonContent = """
        {
          "name": "test-package",
          "version": "1.0.0",
          "dependencies": {
            "lodash": "^4.17.21",
            "axios": "^0.24.0"
          },
          "devDependencies": {
            "jest": "^27.0.0"
          }
        }
        """;

        var packageJsonPath = Path.Combine(_tempDirectory, "package.json");
        await File.WriteAllTextAsync(packageJsonPath, packageJsonContent);

        // Act
        var result = await _service.AnalysePackageJsonDependenciesAsync(packageJsonPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageJsonPath, result.FullLocalPathToPackageJson);
        Assert.NotNull(result.OriginalPackageJsonDependencies);
        Assert.NotNull(result.FullPackageJson);
        
        Assert.Contains("lodash", result.OriginalPackageJsonDependencies.Dependencies?.Keys ?? new Dictionary<string, string>().Keys);
        Assert.Contains("jest", result.OriginalPackageJsonDependencies.DevDependencies?.Keys ?? new Dictionary<string, string>().Keys);
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithNonExistentFile_ShouldThrowException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.json");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _service.AnalysePackageJsonDependenciesAsync(nonExistentPath));
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithNonJsonFile_ShouldThrowException()
    {
        // Arrange
        var textFilePath = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(textFilePath, "This is not a JSON file");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AnalysePackageJsonDependenciesAsync(textFilePath));
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithEmptyFile_ShouldThrowException()
    {
        // Arrange
        var emptyJsonPath = Path.Combine(_tempDirectory, "empty.json");
        await File.WriteAllTextAsync(emptyJsonPath, "");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AnalysePackageJsonDependenciesAsync(emptyJsonPath));
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithInvalidJson_ShouldThrowException()
    {
        // Arrange
        var invalidJsonPath = Path.Combine(_tempDirectory, "invalid.json");
        await File.WriteAllTextAsync(invalidJsonPath, "{ invalid json }");

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() =>
            _service.AnalysePackageJsonDependenciesAsync(invalidJsonPath));
    }

    [Fact]
    public async Task AnalyseMultiplePackageJsonDependenciesAsync_WithMultipleFiles_ShouldReturnCollection()
    {
        // Arrange
        var subDir1 = Path.Combine(_tempDirectory, "project1");
        var subDir2 = Path.Combine(_tempDirectory, "project2");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        var packageJson1 = """
        {
          "name": "project1",
          "version": "1.0.0",
          "dependencies": {
            "lodash": "^4.17.21"
          }
        }
        """;

        var packageJson2 = """
        {
          "name": "project2",
          "version": "2.0.0",
          "dependencies": {
            "axios": "^0.24.0"
          }
        }
        """;

        await File.WriteAllTextAsync(Path.Combine(subDir1, "package.json"), packageJson1);
        await File.WriteAllTextAsync(Path.Combine(subDir2, "package.json"), packageJson2);

        // Act
        var result = await _service.AnalyseMultiplePackageJsonDependenciesAsync(_tempDirectory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var project1 = result.FirstOrDefault(x => x.OriginalPackageJsonDependencies.Dependencies?.ContainsKey("lodash") == true);
        var project2 = result.FirstOrDefault(x => x.OriginalPackageJsonDependencies.Dependencies?.ContainsKey("axios") == true);
        
        Assert.NotNull(project1);
        Assert.NotNull(project2);
        Assert.Contains("lodash", project1.OriginalPackageJsonDependencies.Dependencies?.Keys ?? new Dictionary<string, string>().Keys);
        Assert.Contains("axios", project2.OriginalPackageJsonDependencies.Dependencies?.Keys ?? new Dictionary<string, string>().Keys);
    }

    [Fact]
    public async Task AnalyseMultiplePackageJsonDependenciesAsync_WithNoPackageJsonFiles_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyDir = Path.Combine(_tempDirectory, "empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var result = await _service.AnalyseMultiplePackageJsonDependenciesAsync(emptyDir);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateExistingPackageJsonDependenciesAsync_WithValidInput_ShouldUpdateFileAndReturnUpdatedLazyPackageJson()
    {
        // Arrange
        var originalPackageJson = """
        {
          "name": "test-package",
          "version": "1.0.0",
          "dependencies": {
            "lodash": "^4.17.21"
          }
        }
        """;

        var packageJsonPath = Path.Combine(_tempDirectory, "package.json");
        await File.WriteAllTextAsync(packageJsonPath, originalPackageJson);

        var originalLazy = await _service.AnalysePackageJsonDependenciesAsync(packageJsonPath);
        
        // Set up the new dependencies
        var newDependencies = new PackageJsonDependencies
        {
            Dependencies = new Dictionary<string, string>
            {
                { "lodash", "^4.18.0" }, // Updated version
                { "axios", "^1.0.0" }    // New dependency
            }
        };

        originalLazy.PotentialNewPackageJsonDependencies = newDependencies;

        // Act
        var result = await _service.UpdateExistingPackageJsonDependenciesAsync(
            originalLazy, packageJsonPath);

        // Assert
        Assert.NotNull(result);
        
        // Verify the file was updated
        var updatedFileContent = await File.ReadAllTextAsync(packageJsonPath);
        Assert.Contains("4.18.0", updatedFileContent);
        Assert.Contains("axios", updatedFileContent);
        
        // Verify the returned object reflects the changes
        Assert.Contains("axios", result.OriginalPackageJsonDependencies.Dependencies?.Keys ?? new Dictionary<string, string>().Keys);
        Assert.Equal("^4.18.0", result.OriginalPackageJsonDependencies.Dependencies?["lodash"]);
    }

    [Fact]
    public async Task UpdateExistingPackageJsonDependenciesAsync_WithNullPotentialNewDependencies_ShouldThrowArgumentNullException()
    {
        // Arrange
        var packageJsonPath = Path.Combine(_tempDirectory, "package.json");
        await File.WriteAllTextAsync(packageJsonPath, "{}");

        var lazyPackageJson = new LazyPackageJson
        {
            FullLocalPathToPackageJson = packageJsonPath,
            OriginalPackageJsonDependencies = new PackageJsonDependencies(),
            FullPackageJson = new Lazy<System.Text.Json.Nodes.JsonObject>(() => 
                System.Text.Json.Nodes.JsonNode.Parse("{}")!.AsObject()),
            PotentialNewPackageJsonDependencies = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UpdateExistingPackageJsonDependenciesAsync(lazyPackageJson, packageJsonPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
