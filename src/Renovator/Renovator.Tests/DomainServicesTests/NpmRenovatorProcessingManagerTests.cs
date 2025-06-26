using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Renovator.Common.Exceptions;
using Renovator.Domain.Models;
using Renovator.Domain.Models.Views;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Concrete;
using Renovator.NpmHttpClient.Abstract;
using Renovator.NpmHttpClient.Models.Request;
using Renovator.NpmHttpClient.Models.Response;

namespace Renovator.Tests.DomainServicesTests;

public class NpmRenovatorProcessingManagerTests
{
    private readonly Mock<INpmJsRegistryHttpClient> _mockNpmHttpClient;
    private readonly Mock<IRepoExplorerService> _mockRepoExplorer;
    private readonly Mock<INpmCommandService> _mockNpmCommandService;
    private readonly NpmRenovatorProcessingManager _service;
    private readonly Fixture _fixture;

    public NpmRenovatorProcessingManagerTests()
    {
        _mockNpmHttpClient = new Mock<INpmJsRegistryHttpClient>();
        _mockRepoExplorer = new Mock<IRepoExplorerService>();
        _mockNpmCommandService = new Mock<INpmCommandService>();
        _fixture = new Fixture();

        _service = new NpmRenovatorProcessingManager(
            _mockNpmHttpClient.Object,
            _mockRepoExplorer.Object,
            NullLogger<NpmRenovatorProcessingManager>.Instance,
            _mockNpmCommandService.Object);
    }

    [Fact]
    public async Task AttemptToRenovateLocalSystemRepoAsync_WithSuccessfulUpgrade_ShouldReturnSuccessOutcome()
    {
        // Arrange
        var upgradeBuilder = _fixture.Create<LocalDependencyUpgradeBuilder>();
        var packageJson = CreateTestLazyPackageJson();
        var npmInstallResult = new ProcessCommandResult { Output = "Success", ExceptionOutput = "" };

        _mockRepoExplorer.Setup(x => x.AnalysePackageJsonDependenciesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(packageJson);

        _mockNpmHttpClient.Setup(x => x.ExecuteAsync(It.IsAny<NpmJsRegistryRequestBody>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestNpmResponse());

        _mockRepoExplorer.Setup(x => x.UpdateExistingPackageJsonDependenciesAsync(It.IsAny<LazyPackageJson>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(packageJson);

        _mockNpmCommandService.Setup(x => x.RunNpmInstallAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(npmInstallResult);

        // Act
        var result = await _service.AttemptToRenovateLocalSystemRepoAsync(upgradeBuilder);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Success", result.Data.Output);
        Assert.Null(result.RenovatorException);
    }

    [Fact]
    public async Task AttemptToRenovateLocalSystemRepoAsync_WithNpmInstallFailure_ShouldReturnFailureOutcome()
    {
        // Arrange
        var upgradeBuilder = _fixture.Create<LocalDependencyUpgradeBuilder>();
        var packageJson = CreateTestLazyPackageJson();
        var npmInstallResult = new ProcessCommandResult { Output = "", ExceptionOutput = "Install failed" };

        _mockRepoExplorer.Setup(x => x.AnalysePackageJsonDependenciesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(packageJson);

        _mockNpmHttpClient.Setup(x => x.ExecuteAsync(It.IsAny<NpmJsRegistryRequestBody>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestNpmResponse());

        _mockRepoExplorer.Setup(x => x.UpdateExistingPackageJsonDependenciesAsync(It.IsAny<LazyPackageJson>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(packageJson);

        _mockNpmCommandService.Setup(x => x.RunNpmInstallAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(npmInstallResult);

        // Act
        var result = await _service.AttemptToRenovateLocalSystemRepoAsync(upgradeBuilder);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.RenovatorException);
        Assert.Equal("Install failed", result.RenovatorException.Message);

        // Verify rollback was attempted
        _mockRepoExplorer.Verify(x => x.UpdateExistingPackageJsonDependenciesAsync(
            It.IsAny<LazyPackageJson>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task AttemptToRenovateLocalSystemRepoAsync_WithException_ShouldReturnFailureOutcomeAndAttemptRollback()
    {
        // Arrange
        var upgradeBuilder = _fixture.Create<LocalDependencyUpgradeBuilder>();
        var packageJson = CreateTestLazyPackageJson();

        _mockRepoExplorer.Setup(x => x.AnalysePackageJsonDependenciesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(packageJson);

        _mockNpmHttpClient.Setup(x => x.ExecuteAsync(It.IsAny<NpmJsRegistryRequestBody>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _service.AttemptToRenovateLocalSystemRepoAsync(upgradeBuilder);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.RenovatorException);
        Assert.Contains("Network error", result.RenovatorException.InnerException?.Message ?? "");

        // Verify rollback was attempted
        _mockRepoExplorer.Verify(x => x.UpdateExistingPackageJsonDependenciesAsync(
            It.IsAny<LazyPackageJson>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync_WithValidPackageJson_ShouldReturnUpgradesView()
    {
        // Arrange
        var filePath = "/test/package.json";
        var packageJson = CreateTestLazyPackageJson();
        var npmResponse = CreateTestNpmResponse();

        _mockRepoExplorer.Setup(x => x.AnalysePackageJsonDependenciesAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(packageJson);

        _mockNpmHttpClient.Setup(x => x.ExecuteAsync(It.IsAny<NpmJsRegistryRequestBody>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(npmResponse);

        // Act
        var result = await _service.GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.AllPackages);
        Assert.True(result.Data.AllPackages.Any());

        // Verify HTTP client was called for each dependency
        _mockNpmHttpClient.Verify(x => x.ExecuteAsync(
            It.IsAny<NpmJsRegistryRequestBody>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync_WithException_ShouldReturnFailureOutcome()
    {
        // Arrange
        var filePath = "/test/package.json";

        _mockRepoExplorer.Setup(x => x.AnalysePackageJsonDependenciesAsync(filePath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Package.json not found"));

        // Act
        var result = await _service.GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.RenovatorException);
        Assert.Contains("Package.json not found", result.RenovatorException.InnerException?.Message ?? "");
    }

    [Fact]
    public async Task AttemptToRenovateLocalSystemRepoAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var upgradeBuilder = _fixture.Create<LocalDependencyUpgradeBuilder>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var result = await _service.AttemptToRenovateLocalSystemRepoAsync(upgradeBuilder, cts.Token);
        
        // Should return failure outcome due to cancellation
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    private LazyPackageJson CreateTestLazyPackageJson()
    {
        var dependencies = new Dictionary<string, string>
        {
            { "lodash", "^4.17.21" },
            { "axios", "^0.24.0" }
        };

        var devDependencies = new Dictionary<string, string>
        {
            { "jest", "^27.0.0" },
            { "@types/node", "^16.0.0" }
        };

        return new LazyPackageJson
        {
            FullLocalPathToPackageJson = "/test/package.json",
            OriginalPackageJsonDependencies = new PackageJsonDependencies
            {
                Dependencies = dependencies,
                DevDependencies = devDependencies
            },
            FullPackageJson = new Lazy<System.Text.Json.Nodes.JsonObject>(() => 
                System.Text.Json.Nodes.JsonNode.Parse("{}")!.AsObject())
        };
    }

    private NpmJsRegistryResponse CreateTestNpmResponse()
    {
        return new NpmJsRegistryResponse
        {
            Objects = new[]
            {
                new NpmJsRegistryResponseSingleObject
                {
                    Package = new NpmJsRegistryResponseSingleObjectPackage
                    {
                        Name = "lodash",
                        Version = "4.17.22",
                        Date = DateTime.UtcNow,
                        Publisher = new NpmJsRegistryResponseSingleObjectUser
                        {
                            Email = "test@example.com",
                            Username = "testuser"
                        }
                    },
                    Updated = DateTime.UtcNow,
                    Dependents = 1000,
                    SearchScore = 1.0m,
                    Downloads = new NpmJsRegistryResponseSingleObjectDownloads
                    {
                        Monthly = 10000,
                        Weekly = 2500
                    },
                    Score = new NpmJsRegistryResponseSingleObjectScore
                    {
                        Final = 0.95m
                    }
                }
            }
        };
    }
}
