using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Renovator.Domain.Models;
using Renovator.Domain.Services.Concrete;
using System.Text.Json;

namespace Renovator.Tests.DomainServicesTests;

public class RepoExplorerServiceTests : IDisposable
{
    private readonly Mock<ILogger<RepoExplorerService>> _mockLogger;
    private readonly RepoExplorerService _sut;
    private readonly Fixture _fixture;
    private readonly string _testDirectory;

    public RepoExplorerServiceTests()
    {
        _mockLogger = new Mock<ILogger<RepoExplorerService>>();
        _sut = new RepoExplorerService(_mockLogger.Object);
        _fixture = new Fixture();
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithValidPackageJson_ShouldReturnLazyPackageJson()
    {
        // Arrange
        var packageJsonContent = CreateTestPackageJsonContent("test-app", "1.0.0");
        var packageJsonPath = Path.Combine(_testDirectory, "package.json");
        await File.WriteAllTextAsync(packageJsonPath, packageJsonContent);

        // Act
        var result = await _sut.AnalysePackageJsonDependenciesAsync(packageJsonPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageJsonPath, result.FullLocalPathToPackageJson);
        Assert.NotNull(result.OriginalPackageJsonDependencies);
        Assert.True(result.OriginalPackageJsonDependencies.Dependencies.ContainsKey("react"));
        Assert.True(result.OriginalPackageJsonDependencies.DevDependencies.ContainsKey("@types/react"));
        Assert.Equal("^17.0.0", result.OriginalPackageJsonDependencies.Dependencies["react"]);
        Assert.Equal("^17.0.0", result.OriginalPackageJsonDependencies.DevDependencies["@types/react"]);
    }

    [Fact]
    public async Task AnalyseMultiplePackageJsonDependenciesAsync_WithMultiplePackageJsons_ShouldReturnAllPackageJsons()
    {
        // Arrange
        var rootPackageJsonPath = Path.Combine(_testDirectory, "package.json");
        var subDir1 = Path.Combine(_testDirectory, "frontend");
        var subDir2 = Path.Combine(_testDirectory, "backend");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);
        
        var frontend1PackageJsonPath = Path.Combine(subDir1, "package.json");
        var backend1PackageJsonPath = Path.Combine(subDir2, "package.json");

        await File.WriteAllTextAsync(rootPackageJsonPath, CreateTestPackageJsonContent("root-app", "1.0.0"));
        await File.WriteAllTextAsync(frontend1PackageJsonPath, CreateTestPackageJsonContent("frontend-app", "2.0.0"));
        await File.WriteAllTextAsync(backend1PackageJsonPath, CreateTestPackageJsonContent("backend-app", "3.0.0"));

        // Act
        var result = await _sut.AnalyseMultiplePackageJsonDependenciesAsync(_testDirectory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        var paths = result.Select(p => p.FullLocalPathToPackageJson).ToArray();
        Assert.Contains(rootPackageJsonPath, paths);
        Assert.Contains(frontend1PackageJsonPath, paths);
        Assert.Contains(backend1PackageJsonPath, paths);

        // Verify each package.json has proper structure
        foreach (var packageJson in result)
        {
            Assert.NotNull(packageJson.OriginalPackageJsonDependencies);
            Assert.True(packageJson.OriginalPackageJsonDependencies.Dependencies.ContainsKey("react"));
            Assert.True(packageJson.OriginalPackageJsonDependencies.DevDependencies.ContainsKey("@types/react"));
        }
    }

    [Fact]
    public async Task AnalyseMultiplePackageJsonDependenciesAsync_WithEmptyDirectory_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyDirectory = Path.Combine(_testDirectory, "empty");
        Directory.CreateDirectory(emptyDirectory);

        // Act
        var result = await _sut.AnalyseMultiplePackageJsonDependenciesAsync(emptyDirectory);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateExistingPackageJsonDependenciesAsync_WithNewDependencies_ShouldUpdateFile()
    {
        // Arrange
        var packageJsonPath = Path.Combine(_testDirectory, "package.json");
        var originalContent = CreateTestPackageJsonContent("test-app", "1.0.0");
        await File.WriteAllTextAsync(packageJsonPath, originalContent);

        var originalPackageJson = await _sut.AnalysePackageJsonDependenciesAsync(packageJsonPath);
        
        // Create updated dependencies
        var updatedPackageJson = originalPackageJson with
        {
            PotentialNewPackageJsonDependencies = new PackageJsonDependencies
            {
                Dependencies = new Dictionary<string, string>
                {
                    { "react", "^18.2.0" }, // Updated version
                    { "lodash", "^4.17.21" }, // Keep existing
                    { "express", "^4.18.2" } // New package
                },
                DevDependencies = new Dictionary<string, string>
                {
                    { "@types/react", "^18.2.0" }, // Updated version
                    { "typescript", "^4.9.0" }, // Keep existing
                    { "@types/node", "^18.0.0" } // New package
                }
            }
        };

        // Act
        var result = await _sut.UpdateExistingPackageJsonDependenciesAsync(updatedPackageJson, packageJsonPath);

        // Assert
        Assert.NotNull(result);
        
        // Verify file was updated
        var updatedContent = await File.ReadAllTextAsync(packageJsonPath);
        var updatedJson = JsonDocument.Parse(updatedContent);
        
        Assert.Equal("^18.2.0", updatedJson.RootElement.GetProperty("dependencies").GetProperty("react").GetString());
        Assert.Equal("^4.18.2", updatedJson.RootElement.GetProperty("dependencies").GetProperty("express").GetString());
        Assert.Equal("^18.2.0", updatedJson.RootElement.GetProperty("devDependencies").GetProperty("@types/react").GetString());
        Assert.Equal("^18.0.0", updatedJson.RootElement.GetProperty("devDependencies").GetProperty("@types/node").GetString());
    }

    [Theory]
    [InlineData("my-test-app", "2.1.0")]
    [InlineData("frontend-service", "1.5.3")]
    [InlineData("backend-api", "3.0.0-beta.1")]
    public async Task AnalysePackageJsonDependenciesAsync_WithDifferentAppNames_ShouldParseCorrectly(string appName, string version)
    {
        // Arrange
        var packageJsonContent = CreateTestPackageJsonContent(appName, version);
        var packageJsonPath = Path.Combine(_testDirectory, $"{appName}-package.json");
        await File.WriteAllTextAsync(packageJsonPath, packageJsonContent);

        // Act
        var result = await _sut.AnalysePackageJsonDependenciesAsync(packageJsonPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageJsonPath, result.FullLocalPathToPackageJson);
        
        var nameProperty = result.FullPackageJson.Value["name"]?.ToString();
        var versionProperty = result.FullPackageJson.Value["version"]?.ToString();
        
        Assert.Equal(appName, nameProperty);
        Assert.Equal(version, versionProperty);
    }

    [Fact]
    public async Task AnalysePackageJsonDependenciesAsync_WithComplexDependencyStructure_ShouldParseAllDependencies()
    {
        // Arrange
        var complexPackageJson = @"{
  ""name"": ""complex-app"",
  ""version"": ""1.0.0"",
  ""dependencies"": {
    ""react"": ""^18.2.0"",
    ""@emotion/react"": ""^11.10.0"",
    ""@mui/material"": ""^5.10.0"",
    ""axios"": ""^0.27.2"",
    ""date-fns"": ""^2.29.0""
  },
  ""devDependencies"": {
    ""@types/react"": ""^18.0.0"",
    ""@types/react-dom"": ""^18.0.0"",
    ""@typescript-eslint/eslint-plugin"": ""^5.40.0"",
    ""eslint"": ""^8.25.0"",
    ""prettier"": ""^2.7.0"",
    ""vite"": ""^3.1.0""
  }
}";
        var packageJsonPath = Path.Combine(_testDirectory, "complex-package.json");
        await File.WriteAllTextAsync(packageJsonPath, complexPackageJson);

        // Act
        var result = await _sut.AnalysePackageJsonDependenciesAsync(packageJsonPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.OriginalPackageJsonDependencies.Dependencies.Count);
        Assert.Equal(6, result.OriginalPackageJsonDependencies.DevDependencies.Count);
        
        // Check specific packages
        Assert.Equal("^18.2.0", result.OriginalPackageJsonDependencies.Dependencies["react"]);
        Assert.Equal("^11.10.0", result.OriginalPackageJsonDependencies.Dependencies["@emotion/react"]);
        Assert.Equal("^5.40.0", result.OriginalPackageJsonDependencies.DevDependencies["@typescript-eslint/eslint-plugin"]);
        Assert.Equal("^3.1.0", result.OriginalPackageJsonDependencies.DevDependencies["vite"]);
    }

    [Fact]
    public async Task UpdateExistingPackageJsonDependenciesAsync_WithOnlyDependencyUpdates_ShouldNotChangeDevDependencies()
    {
        // Arrange
        var packageJsonPath = Path.Combine(_testDirectory, "package.json");
        var originalContent = CreateTestPackageJsonContent("test-app", "1.0.0");
        await File.WriteAllTextAsync(packageJsonPath, originalContent);

        var originalPackageJson = await _sut.AnalysePackageJsonDependenciesAsync(packageJsonPath);
        
        var updatedPackageJson = originalPackageJson with
        {
            PotentialNewPackageJsonDependencies = new PackageJsonDependencies
            {
                Dependencies = new Dictionary<string, string>
                {
                    { "react", "^18.2.0" }, // Updated version only
                    { "lodash", "^4.17.21" } // Keep existing
                },
                DevDependencies = originalPackageJson.OriginalPackageJsonDependencies.DevDependencies // Keep original
            }
        };

        // Act
        var result = await _sut.UpdateExistingPackageJsonDependenciesAsync(updatedPackageJson, packageJsonPath);

        // Assert
        var updatedContent = await File.ReadAllTextAsync(packageJsonPath);
        var updatedJson = JsonDocument.Parse(updatedContent);
        
        // Dependencies should be updated
        Assert.Equal("^18.2.0", updatedJson.RootElement.GetProperty("dependencies").GetProperty("react").GetString());
        
        // DevDependencies should remain the same
        Assert.Equal("^17.0.0", updatedJson.RootElement.GetProperty("devDependencies").GetProperty("@types/react").GetString());
        Assert.Equal("^4.9.0", updatedJson.RootElement.GetProperty("devDependencies").GetProperty("typescript").GetString());
    }

    private string CreateTestPackageJsonContent(string name, string version)
    {
        return $@"{{
  ""name"": ""{name}"",
  ""version"": ""{version}"",
  ""dependencies"": {{
    ""react"": ""^17.0.0"",
    ""lodash"": ""^4.17.21""
  }},
  ""devDependencies"": {{
    ""@types/react"": ""^17.0.0"",
    ""typescript"": ""^4.9.0""
  }}
}}";
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
