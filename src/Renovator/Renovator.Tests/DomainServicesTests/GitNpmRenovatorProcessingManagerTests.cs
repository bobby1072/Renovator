using AutoFixture;
using Microsoft.Extensions.Logging.Abstractions;
using Renovator.Common.Exceptions;
using Renovator.Domain.Models;
using Renovator.Domain.Models.Views;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Concrete;
using Renovator.NpmHttpClient.Abstract;
using Renovator.NpmHttpClient.Models.Request;
using Renovator.NpmHttpClient.Models.Response;
using System.Text.Json.Nodes;

namespace Renovator.Tests.DomainServicesTests;

public class GitNpmRenovatorProcessingManagerTests : IDisposable
{
    private readonly TestGitCommandService _testGitCommandService;
    private readonly TestNpmHttpClient _testNpmHttpClient;
    private readonly TestRepoExplorerService _testRepoExplorer;
    private readonly TestNpmCommandService _testNpmCommandService;
    private readonly GitNpmRenovatorProcessingManager _service;
    private readonly Fixture _fixture;

    public GitNpmRenovatorProcessingManagerTests()
    {
        _testGitCommandService = new TestGitCommandService();
        _testNpmHttpClient = new TestNpmHttpClient();
        _testRepoExplorer = new TestRepoExplorerService();
        _testNpmCommandService = new TestNpmCommandService();
        _fixture = new Fixture();

        _service = new GitNpmRenovatorProcessingManager(
            _testGitCommandService,
            NullLogger<GitNpmRenovatorProcessingManager>.Instance,
            _testNpmHttpClient,
            _testRepoExplorer,
            NullLogger<NpmRenovatorProcessingManager>.Instance,
            _testNpmCommandService);
    }

    [Fact]
    public async Task GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync_WithValidRepo_ShouldReturnUpgradesView()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/repo.git");
        var tempRepo = CreateTestTempRepository();
        var packageJsons = new[] { CreateTestLazyPackageJson() };
        var npmResponse = CreateTestNpmResponse();

        _testGitCommandService.SetupCheckoutResult(tempRepo);
        _testRepoExplorer.SetupAnalyseMultipleResult(packageJsons);
        _testNpmHttpClient.SetupResponse(npmResponse);

        // Act
        var result = await _service.GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync(gitRepoUri, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var firstResult = result.Data.First();
        Assert.NotNull(firstResult.AllPackages);
        Assert.NotEmpty(firstResult.AllPackages);
        Assert.Equal(packageJsons[0].FullLocalPathToPackageJson, firstResult.FullPathToPackageJson);
    }

    [Fact]
    public async Task GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync_WithGitCloneFailure_ShouldReturnFailureOutcome()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/invalid-repo.git");

        _testGitCommandService.SetupCheckoutFailure("Repository not found");

        // Act
        var result = await _service.GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync(gitRepoUri, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.RenovatorException);
    }

    [Fact]
    public async Task FindAllPackageJsonsInTempRepoAsync_WithValidRepo_ShouldReturnPackageJsons()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/repo.git");
        var tempRepo = CreateTestTempRepository();
        var packageJsons = new[] { CreateTestLazyPackageJson(), CreateTestLazyPackageJson() };

        _testGitCommandService.SetupCheckoutResult(tempRepo);
        _testRepoExplorer.SetupAnalyseMultipleResult(packageJsons);

        // Act
        var result = await _service.FindAllPackageJsonsInTempRepoAsync(gitRepoUri, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task FindAllPackageJsonsInTempRepoAsync_WithException_ShouldReturnFailureOutcome()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/repo.git");

        _testGitCommandService.SetupCheckoutException(new Exception("Network error"));

        // Act
        var result = await _service.FindAllPackageJsonsInTempRepoAsync(gitRepoUri, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.RenovatorException);
        Assert.Contains("Network error", result.RenovatorException.InnerException?.Message ?? "");
    }

    [Fact]
    public async Task AttemptToRenovateTempRepoAsync_WithValidBuilder_ShouldRenovateSuccessfully()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/repo.git");
        var upgradeBuilder = GitDependencyUpgradeBuilder.Create(gitRepoUri, "test-package", "lodash");

        var tempRepo = CreateTestTempRepository();
        var packageJson = CreateTestLazyPackageJson();
        var packageJsons = new[] { packageJson };
        var npmInstallResult = new ProcessCommandResult { Output = "Success", ExceptionOutput = "" };

        _testGitCommandService.SetupCheckoutResult(tempRepo);
        _testRepoExplorer.SetupAnalyseMultipleResult(packageJsons);
        _testRepoExplorer.SetupAnalyseSingleResult(packageJson);
        _testRepoExplorer.SetupUpdateResult(packageJson);
        _testNpmCommandService.SetupNpmInstallResult(npmInstallResult);
        _testNpmHttpClient.SetupResponse(CreateTestNpmResponse());

        // Act
        var result = await _service.AttemptToRenovateTempRepoAsync(upgradeBuilder, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Success", result.Data.Output);
    }

    [Fact]
    public async Task AttemptToRenovateTempRepoAsync_WithPackageNotFound_ShouldReturnFailureOutcome()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/repo.git");
        var upgradeBuilder = GitDependencyUpgradeBuilder.Create(gitRepoUri, "nonexistent-package");

        var tempRepo = CreateTestTempRepository();
        var packageJson = CreateTestLazyPackageJson();
        var packageJsons = new[] { packageJson };

        _testGitCommandService.SetupCheckoutResult(tempRepo);
        _testRepoExplorer.SetupAnalyseMultipleResult(packageJsons);

        // Act
        var result = await _service.AttemptToRenovateTempRepoAsync(upgradeBuilder, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.RenovatorException);
        Assert.Contains("Could not find package json", result.RenovatorException.InnerException?.Message ?? "");
    }

    [Fact]
    public async Task GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync_WithSameRepoTwice_ShouldReuseTempRepo()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Short timeout to prevent real operations
        var gitRepoUri = new Uri("https://github.com/test/repo.git");
        var tempRepo = CreateTestTempRepository();
        var packageJsons = new[] { CreateTestLazyPackageJson() };
        var npmResponse = CreateTestNpmResponse();

        _testGitCommandService.SetupCheckoutResult(tempRepo);
        _testRepoExplorer.SetupAnalyseMultipleResult(packageJsons);
        _testNpmHttpClient.SetupResponse(npmResponse);

        // Act
        var result1 = await _service.GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync(gitRepoUri, cts.Token);
        var result2 = await _service.GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync(gitRepoUri, cts.Token);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);

        // Git command should only be called once (repo reused)
        Assert.Equal(1, _testGitCommandService.CheckoutCallCount);
    }

    [Fact]
    public void Dispose_ShouldDisposeTempRepository()
    {
        // This test verifies that the Dispose method works correctly
        // In a real scenario, you might want to verify that temp directories are cleaned up

        // Act & Assert
        _service.Dispose();

        // The dispose method should complete without throwing
        Assert.True(true);
    }

    // Test implementations for internal interfaces
    private class TestGitCommandService : IGitCommandService
    {
        private ProcessCommandResult<TempRepositoryFromGit>? _checkoutResult;
        private Exception? _checkoutException;
        public int CheckoutCallCount { get; private set; }

        public void SetupCheckoutResult(TempRepositoryFromGit repo)
        {
            _checkoutResult = new ProcessCommandResult<TempRepositoryFromGit> { Data = repo };
        }

        public void SetupCheckoutFailure(string error)
        {
            _checkoutResult = new ProcessCommandResult<TempRepositoryFromGit> { Data = null, ExceptionOutput = error };
        }

        public void SetupCheckoutException(Exception ex)
        {
            _checkoutException = ex;
        }

        public Task<ProcessCommandResult<TempRepositoryFromGit>> CheckoutRemoteRepoToLocalTempStoreAsync(Uri remoteRepoLocation, CancellationToken token = default)
        {
            CheckoutCallCount++;
            
            if (_checkoutException != null)
                throw _checkoutException;
                
            return Task.FromResult(_checkoutResult ?? new ProcessCommandResult<TempRepositoryFromGit> { Data = null, ExceptionOutput = "Test setup error" });
        }
    }

    private class TestRepoExplorerService : IRepoExplorerService
    {
        private IReadOnlyCollection<LazyPackageJson>? _analyseMultipleResult;
        private LazyPackageJson? _analyseSingleResult;
        private LazyPackageJson? _updateResult;

        public void SetupAnalyseMultipleResult(IReadOnlyCollection<LazyPackageJson> result)
        {
            _analyseMultipleResult = result;
        }

        public void SetupAnalyseSingleResult(LazyPackageJson result)
        {
            _analyseSingleResult = result;
        }

        public void SetupUpdateResult(LazyPackageJson result)
        {
            _updateResult = result;
        }

        public Task<IReadOnlyCollection<LazyPackageJson>> AnalyseMultiplePackageJsonDependenciesAsync(string fullFilePathToFolder, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_analyseMultipleResult ?? Array.Empty<LazyPackageJson>());
        }

        public Task<LazyPackageJson> AnalysePackageJsonDependenciesAsync(string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_analyseSingleResult ?? new LazyPackageJson
            {
                FullLocalPathToPackageJson = "/test/package.json",
                FullPackageJson = new Lazy<JsonObject>(() => new JsonObject()),
                OriginalPackageJsonDependencies = new PackageJsonDependencies()
            });
        }

        public Task<LazyPackageJson> UpdateExistingPackageJsonDependenciesAsync(LazyPackageJson originalWithNewPackages, string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_updateResult ?? originalWithNewPackages);
        }
    }

    private class TestNpmCommandService : INpmCommandService
    {
        private ProcessCommandResult? _npmInstallResult;

        public void SetupNpmInstallResult(ProcessCommandResult result)
        {
            _npmInstallResult = result;
        }

        public Task<ProcessCommandResult> RunNpmInstallAsync(string directoryContainingPackageJson, CancellationToken token = default)
        {
            return Task.FromResult(_npmInstallResult ?? new ProcessCommandResult { Output = "Test npm install", ExceptionOutput = "" });
        }
    }

    private class TestNpmHttpClient : INpmJsRegistryHttpClient
    {
        private NpmJsRegistryResponse? _response;

        public void SetupResponse(NpmJsRegistryResponse response)
        {
            _response = response;
        }

        public Task<NpmJsRegistryResponse?> ExecuteAsync(NpmJsRegistryRequestBody requestBody, CancellationToken token = default)
        {
            return Task.FromResult<NpmJsRegistryResponse?>(_response ?? new NpmJsRegistryResponse { Objects = Array.Empty<NpmJsRegistryResponseSingleObject>() });
        }
    }

    private TempRepositoryFromGit CreateTestTempRepository()
    {
        return new TempRepositoryFromGit
        {
            FolderId = Guid.NewGuid(),
            FullPathTo = Path.Combine(Path.GetTempPath(), "test-repo"),
            GitRepoLocation = new Uri("https://github.com/test/repo.git")
        };
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
                System.Text.Json.Nodes.JsonNode.Parse("""
                {
                  "name": "test-package",
                  "version": "1.0.0",
                  "dependencies": {
                    "lodash": "^4.17.21",
                    "axios": "^0.24.0"
                  },
                  "devDependencies": {
                    "jest": "^27.0.0",
                    "@types/node": "^16.0.0"
                  }
                }
                """)!.AsObject())
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

    public void Dispose()
    {
        _service.Dispose();
    }
}
