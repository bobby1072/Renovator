using BT.Common.FastArray.Proto;
using BT.Common.OperationTimer.Proto;
using Microsoft.Extensions.Logging;
using Npm.Renovator.Application.Models;
using Npm.Renovator.Application.Services.Abstract;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;
using Npm.Renovator.RepoReader.Abstract;
using System.Text.Json;

namespace Npm.Renovator.Application.Services.Concrete;

internal class NpmRenovatorProcessingManager: INpmRenovatorProcessingManager
{
    private readonly INpmJsRegistryHttpClient _npmJsRegistryHttpClient;
    private readonly IRepoReaderService _reader;
    private readonly ILogger<NpmRenovatorProcessingManager> _logger;
    
    public NpmRenovatorProcessingManager(INpmJsRegistryHttpClient npmJsRegistryHttpClient, IRepoReaderService reader,
        ILogger<NpmRenovatorProcessingManager> logger)
    {
        _npmJsRegistryHttpClient = npmJsRegistryHttpClient;
        _reader = reader;
        _logger = logger;
    }

    public async Task<CurrentPackageVersionsAndPotentialUpgradesView> GetCurrentPackageVersionAndPotentialUpgradesViewAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var currentPackages = await _reader.AnalysePackageJsonDependenciesAsync(filePath, cancellationToken);


        var potentialNewPackages = await GetPotentialNewPackagesFromRegistry(currentPackages.Dependencies.Union(currentPackages.DevDependencies).ToDictionary());
        var potentialUpgradesViewList = new List<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage>()
            .Union(GetListOfPotentialNewPackages(currentPackages.Dependencies, potentialNewPackages))
            .Union(GetListOfPotentialNewPackages(currentPackages.Dependencies, potentialNewPackages)).ToArray();

        return new CurrentPackageVersionsAndPotentialUpgradesView { AllPackages = potentialUpgradesViewList };
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
    private static IEnumerable<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> GetListOfPotentialNewPackages(Dictionary<string, string> dependencyList, IReadOnlyCollection<NpmJsRegistryResponseSingleObject> foundPackagesFromRegistry)
    {
        foreach(var package in dependencyList)
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
}