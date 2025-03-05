using BT.Common.FastArray.Proto;
using BT.Common.OperationTimer.Proto;
using Microsoft.Extensions.Logging;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;
using Npm.Renovator.Domain.Services.Abstract;
using System.Text.Json;
using Npm.Renovator.Common.Exceptions;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Concrete;

internal class NpmRenovatorProcessingManager : INpmRenovatorProcessingManager
{
    private readonly INpmJsRegistryHttpClient _npmJsRegistryHttpClient;
    private readonly IRepoReaderService _reader;
    private readonly ILogger<NpmRenovatorProcessingManager> _logger;
    private readonly INpmCommandService _npmCommandService;
    public NpmRenovatorProcessingManager(INpmJsRegistryHttpClient npmJsRegistryHttpClient, 
        IRepoReaderService reader,
        ILogger<NpmRenovatorProcessingManager> logger,
        INpmCommandService npmCommandService)
    {
        _npmJsRegistryHttpClient = npmJsRegistryHttpClient;
        _reader = reader;
        _logger = logger;
        _npmCommandService = npmCommandService;
    }

    public async Task<RenovatorOutcome<NpmCommandResults>> AttemptToRenovateRepoAsync(DependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysedDependencies = await _reader.AnalysePackageJsonDependenciesAsync(upgradeBuilder.FilePath, cancellationToken);

            var upgradedDepends = await Task.WhenAll(UpgradeDependencyDict(analysedDependencies.Dependencies, upgradeBuilder, cancellationToken),
                UpgradeDependencyDict(analysedDependencies.DevDependencies, upgradeBuilder, cancellationToken));

            analysedDependencies.Dependencies = upgradedDepends.First();
            analysedDependencies.DevDependencies = upgradedDepends.Last();
            
            await _reader.UpdateExistingPackageJsonDependenciesAsync(analysedDependencies, upgradeBuilder.FilePath, cancellationToken);
            
            var npmIResult = await _npmCommandService.RunNpmInstallAsync(upgradeBuilder.FilePath, cancellationToken);

            return new RenovatorOutcome<NpmCommandResults>
            {
                RenovatorException = string.IsNullOrEmpty(npmIResult.Exception)
                    ? null
                    : new RenovatorException(npmIResult.Exception),
                Data = npmIResult
            };
        }
        catch (Exception ex)
        {
            return new RenovatorOutcome<NpmCommandResults>
            {
                RenovatorException = GetRenovatorException(nameof(AttemptToRenovateRepoAsync), ex)
            };
        }
    }
    public async Task<RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>> GetCurrentPackageVersionAndPotentialUpgradesViewAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentPackages = await _reader.AnalysePackageJsonDependenciesAsync(filePath, cancellationToken);


            var potentialNewPackages = await GetPotentialNewPackagesFromRegistry(currentPackages.Dependencies
                .Union(currentPackages.DevDependencies).ToDictionary());
            
            var potentialUpgradesViewList = new List<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage>()
                .Union(GetListOfPotentialNewPackages(currentPackages.Dependencies, potentialNewPackages))
                .Union(GetListOfPotentialNewPackages(currentPackages.DevDependencies, potentialNewPackages)).ToArray();

            return new RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>
            {
                Data = new CurrentPackageVersionsAndPotentialUpgradesView { AllPackages = potentialUpgradesViewList }
            };
        }
        catch (Exception ex)
        {
            return new RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>
            {
                RenovatorException = GetRenovatorException(
                    nameof(GetCurrentPackageVersionAndPotentialUpgradesViewAsync),
                    ex)
            };
        }
    }
    private async Task<IReadOnlyCollection<NpmJsRegistryResponseSingleObject>> GetPotentialNewPackagesFromRegistry(Dictionary<string, string> packagesToCheck)
    {
        var jobList = packagesToCheck.DistinctBy(x => x.Key).FastArraySelect(x => _npmJsRegistryHttpClient.ExecuteAsync(
                new NpmJsRegistryRequestBody { Text = x.Key, Size = 1 }
            ));

        var (timeTaken, finishedJobs) = await OperationTimerUtils.TimeWithResultsAsync(() => Task.WhenAll(jobList));

        _logger.LogDebug("Npm Requests completed in {TimeTaken}ms with responses: {ResponseArray}",
            timeTaken,
            JsonSerializer.Serialize(finishedJobs));


        return finishedJobs.FastArrayWhere(x => x is not null).SelectMany(x => x!.Objects).ToArray();
    }

    private static RenovatorException GetRenovatorException(string opName, Exception? innerException = null)
    {
        return new RenovatorException($"Exception occured during execution of {opName}",
            innerException);
    }
    private static IEnumerable<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> GetListOfPotentialNewPackages(Dictionary<string, string> dependencyList, IReadOnlyCollection<NpmJsRegistryResponseSingleObject> foundPackagesFromRegistry)
    {
        foreach (var package in dependencyList)
        {
            yield return new CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage
            {
                NameOnNpm = package.Key,
                CurrentVersion = package.Value,
                PotentialNewVersions = foundPackagesFromRegistry
                    .FastArrayWhere(x => x.Package.Name == package.Key)
                    .Select(x => new CurrentPackageVersionsAndPotentialUpgradesViewPotentialNewVersion
                    {
                        CurrentVersion = x.Package.Version,
                        ReleaseDate = x.Updated
                    })
                    .ToArray()
            };
        }
    }

    private async Task<Dictionary<string, string>> UpgradeDependencyDict(Dictionary<string, string> dependencyDict, DependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken)
    {
        var newVersionJobList = new List<Task<(string Name,string Version)>>();
        
        foreach (var dept in dependencyDict)
        {
            var foundUpgrade = upgradeBuilder.GetUpgradeFor(dept.Key);
            if (foundUpgrade == null)
            {
                continue;
            }
            var newVersionJob = async () => string.IsNullOrEmpty(foundUpgrade.Value.Value) ? (dept.Key,(await
                        _npmJsRegistryHttpClient
                            .ExecuteAsync(new NpmJsRegistryRequestBody { Text = dept.Key, Size = 1 }, cancellationToken))?.Objects
                    .FirstOrDefault()?.Package.Version ?? dept.Value) : (dept.Key, foundUpgrade.Value.Value);

            newVersionJobList.Add(newVersionJob.Invoke());
        }
        var versionJobResult = await Task.WhenAll(newVersionJobList);
        
        var dupedDict = dependencyDict.ToDictionary();
        foreach (var vers in versionJobResult)
        {
            dupedDict[vers.Name] = vers.Version;
        }
        
        return dupedDict;
    }
}