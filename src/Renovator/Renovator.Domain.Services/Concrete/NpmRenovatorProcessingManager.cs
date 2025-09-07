using BT.Common.FastArray.Proto;
using BT.Common.OperationTimer.Proto;
using Microsoft.Extensions.Logging;
using Renovator.Domain.Models;
using Renovator.Domain.Models.Views;
using System.Text.Json;
using Renovator.Common.Exceptions;
using Renovator.Common.Extensions;
using Renovator.Common.Helpers;
using Renovator.Domain.Services.Abstract;
using Renovator.NpmHttpClient.Abstract;
using Renovator.NpmHttpClient.Models.Request;
using Renovator.NpmHttpClient.Models.Response;

namespace Renovator.Domain.Services.Concrete;

internal class NpmRenovatorProcessingManager : INpmRenovatorProcessingManager
{
    protected readonly IRepoExplorerService _reader;
    protected readonly IProcessExecutor _processExecutor;
    private readonly ILogger<NpmRenovatorProcessingManager> _logger;
    private readonly INpmJsRegistryHttpClient _npmJsRegistryHttpClient;
    public NpmRenovatorProcessingManager(INpmJsRegistryHttpClient npmJsRegistryHttpClient,
        IRepoExplorerService reader,
        ILogger<NpmRenovatorProcessingManager> logger,
        IProcessExecutor processExecutor)
    {
        _npmJsRegistryHttpClient = npmJsRegistryHttpClient;
        _reader = reader;
        _logger = logger;
        _processExecutor = processExecutor;
    }

    public async Task<RenovatorOutcome<ProcessCommandResult>> AttemptToRenovateLocalSystemRepoAsync(LocalDependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken = default)
    {
        LazyPackageJson? analysedDependencies = null;
        try
        {
            analysedDependencies = await _reader.AnalysePackageJsonDependenciesAsync(upgradeBuilder.LocalSystemFilePathToJson, cancellationToken);

            var upgradedDepends = await Task.WhenAll(UpgradeDependencyDict(analysedDependencies.OriginalPackageJsonDependencies.Dependencies, upgradeBuilder, cancellationToken),
                UpgradeDependencyDict(analysedDependencies.OriginalPackageJsonDependencies.DevDependencies, upgradeBuilder, cancellationToken));

            analysedDependencies.PotentialNewPackageJsonDependencies = new PackageJsonDependencies
            {
                Dependencies = upgradedDepends.First(),
                DevDependencies = upgradedDepends.Last()
            };
            await _reader.UpdateExistingPackageJsonDependenciesAsync(analysedDependencies, upgradeBuilder.LocalSystemFilePathToJson, cancellationToken);

            var npmIResult = await _processExecutor.RunCommandAsync<NpmInstallProcessCommand, string, ProcessCommandResult>(upgradeBuilder.LocalSystemFilePathToJson.GetFolderSpaceFromFilePath(), cancellationToken);

            var modelToReturn = new RenovatorOutcome<ProcessCommandResult>
            {
                RenovatorException = string.IsNullOrEmpty(npmIResult.ExceptionOutput)
                    ? null
                    : new RenovatorException(npmIResult.ExceptionOutput),
                Data = npmIResult
            };

            if (!modelToReturn.IsSuccess)
            {
                await AttemptToRollbackRepo(upgradeBuilder.LocalSystemFilePathToJson, analysedDependencies, cancellationToken);
            }

            return modelToReturn;
        }
        catch (Exception ex)
        {
            if (analysedDependencies is not null)
            {
                await AttemptToRollbackRepo(upgradeBuilder.LocalSystemFilePathToJson, analysedDependencies, cancellationToken);
            }

            var renovatorException = RenovatorExceptionHelper.CreateRenovatorException(nameof(AttemptToRenovateLocalSystemRepoAsync), ex);

            LogRenovatorException(renovatorException);
            
            return new RenovatorOutcome<ProcessCommandResult>
            {
                RenovatorException = renovatorException
            };
        }
    }
    public async Task<RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>> GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentPackages = await _reader.AnalysePackageJsonDependenciesAsync(localSystemFilePathToPackageJson, cancellationToken);


            var potentialNewPackages = await GetPotentialNewPackagesFromRegistry(currentPackages.OriginalPackageJsonDependencies.Dependencies
                .Union(currentPackages.OriginalPackageJsonDependencies.DevDependencies).ToDictionary());

            var potentialUpgradesViewList = new List<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage>()
                .Union(GetListOfPotentialNewPackages(currentPackages.OriginalPackageJsonDependencies.Dependencies, potentialNewPackages))
                .Union(GetListOfPotentialNewPackages(currentPackages.OriginalPackageJsonDependencies.DevDependencies, potentialNewPackages)).ToArray();

            return new RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>
            {
                Data = new CurrentPackageVersionsAndPotentialUpgradesView { AllPackages = potentialUpgradesViewList }
            };
        }
        catch (Exception ex)
        {
            var renovatorException = RenovatorExceptionHelper.CreateRenovatorException(
                    nameof(GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync),
                    ex);

            LogRenovatorException(renovatorException);

            return new RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>
            {
                RenovatorException = renovatorException
            };
        }
    }
    protected async Task<IReadOnlyCollection<NpmJsRegistryResponseSingleObject>> GetPotentialNewPackagesFromRegistry(Dictionary<string, string> packagesToCheck)
    {
        var jobList = packagesToCheck.DistinctBy(x => x.Key).FastArraySelect(x => _npmJsRegistryHttpClient.ExecuteAsync(
                new NpmJsRegistryRequestBody { Text = x.Key, Size = 1 }
            ));

        var (timeTaken, finishedJobs) = await OperationTimerUtils.TimeWithResultsAsync(() => Task.WhenAll(jobList));

        _logger.LogDebug("Npm Requests completed in {TimeTaken}ms with responses: {@ResponseArray}",
            timeTaken,
            finishedJobs);


        return finishedJobs.FastArrayWhere(x => x is not null).SelectMany(x => x!.Objects).ToArray();
    }
    protected virtual void LogRenovatorException(RenovatorException ex)
    {
        _logger.LogError(ex, "NpmRenovatorProcessingManager caught an exception with the inner message: {InnerMessage}",
            ex.InnerException?.Message);
    }
    protected static IEnumerable<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> GetListOfPotentialNewPackages(Dictionary<string, string> dependencyList, IReadOnlyCollection<NpmJsRegistryResponseSingleObject> foundPackagesFromRegistry)
    {
        foreach (var package in dependencyList)
        {
            yield return new CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage
            {
                NameOnNpm = package.Key,
                CurrentVersion = package.Value,
                PotentialNewVersions = foundPackagesFromRegistry
                    .FastArrayWhere(x => x.Package.Name == package.Key && x.Package.Version != package.Value.Replace("^", ""))
                    .FastArraySelect(x => new CurrentPackageVersionsAndPotentialUpgradesViewPotentialNewVersion
                    {
                        CurrentVersion = x.Package.Version,
                        ReleaseDate = x.Updated
                    })
                    .ToArray()
            };
        }
    }
    private async Task AttemptToRollbackRepo(string filePath, LazyPackageJson originalDependencies,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Attempting to rollback repo at {FilePath}", filePath);
            await _reader.UpdateExistingPackageJsonDependenciesAsync(originalDependencies, filePath, cancellationToken);
            await _processExecutor.RunCommandAsync<NpmInstallProcessCommand, string, ProcessCommandResult>(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback repo at {FilePath}", filePath);
        }
    }
    private async Task<Dictionary<string, string>> UpgradeDependencyDict(Dictionary<string, string> dependencyDict, LocalDependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken)
    {
        var newVersionJobList = new List<Task<(string Name, string Version)>>();

        foreach (var dept in dependencyDict)
        {
            var foundUpgrade = upgradeBuilder.GetUpgradeFor(dept.Key);
            if (foundUpgrade == null)
            {
                continue;
            }
            var newVersionJob = async () => string.IsNullOrEmpty(foundUpgrade.Value.Value) ? (dept.Key, (await
                        _npmJsRegistryHttpClient
                            .ExecuteAsync(new NpmJsRegistryRequestBody { Text = dept.Key, Size = 1 }, cancellationToken))?.Objects
                    .FirstOrDefault()?.Package.Version ?? dept.Value) : (dept.Key, foundUpgrade.Value.Value);

            newVersionJobList.Add(newVersionJob.Invoke());
        }
        var (timeTaken, versionJobResult) = await OperationTimerUtils.TimeWithResultsAsync(() => Task.WhenAll(newVersionJobList));

        _logger.LogDebug("It took {TimeTaken}ms to get package upgrades from client with results: {@Results}",
            timeTaken,
            versionJobResult
        );
        
        var dupedDict = dependencyDict.ToDictionary();
        foreach (var vers in versionJobResult)
        {
            dupedDict[vers.Name] = vers.Version;
        }

        return dupedDict;
    }

}